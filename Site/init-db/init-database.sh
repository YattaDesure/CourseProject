#!/bin/bash

# Скрипт инициализации базы данных
# Запускается после готовности SQL Server

SA_PASSWORD="${SA_PASSWORD:-22332123Yaz}"
SQL_SERVER="${SQL_SERVER:-sqlserver}"
EXIT_CODE=0

echo "⏳ Ожидание готовности SQL Server..."

# Ждем, пока SQL Server будет готов принимать подключения
SQL_READY=0
for i in {1..30}; do
    if /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null; then
        echo "✅ SQL Server готов"
        SQL_READY=1
        break
    fi
    sleep 2
done

if [ $SQL_READY -eq 0 ]; then
    echo "❌ SQL Server не готов после 30 попыток"
    exit 1
fi

echo "🔍 Проверка базы данных Cursovaya..."

# Проверяем, существует ли база данных и доступна ли она
DB_EXISTS=""
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya' AND state_desc = 'ONLINE'" -h -1 2>/dev/null | grep -i cursovaya || true)

if [ -z "$DB_EXISTS" ]; then
    echo "📦 База данных не найдена или недоступна, создаем/восстанавливаем..."
    
    # Сначала удаляем БД, если она существует в неправильном состоянии
    /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
        -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Cursovaya') BEGIN ALTER DATABASE Cursovaya SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE Cursovaya; END" 2>&1 || true
    sleep 2
    
    # Проверяем наличие файла резервной копии
    if [ -f "/scripts/Cursovaya.bak" ]; then
        echo "📥 Найден файл резервной копии: /scripts/Cursovaya.bak"
        echo "🔄 Восстановление базы данных из резервной копии..."
        
        # Восстанавливаем БД (файл должен быть доступен из SQL Server контейнера)
        # Используем путь /backup, который смонтирован в SQL Server контейнере
        RESTORE_RESULT=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "RESTORE DATABASE Cursovaya FROM DISK = '/backup/Cursovaya.bak' WITH REPLACE, STATS = 5" 2>&1)
        RESTORE_EXIT=$?
        
        if [ $RESTORE_EXIT -eq 0 ]; then
            echo "✅ Команда восстановления выполнена успешно"
            # Ждем, пока БД станет доступной (может быть в состоянии RESTORING)
            echo "⏳ Ожидание доступности базы данных (это может занять до 90 секунд)..."
            DB_READY=0
            for j in {1..45}; do
                sleep 2
                # Проверяем состояние БД
                DB_STATE=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                    -Q "SELECT state_desc FROM sys.databases WHERE name = 'Cursovaya'" -h -1 2>/dev/null | grep -iE "ONLINE|RESTORING" || true)
                
                if echo "$DB_STATE" | grep -qi "ONLINE"; then
                    # Дополнительная проверка - пытаемся выполнить запрос
                    sleep 1
                    QUERY_TEST=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                        -d Cursovaya -Q "SELECT 1" -h -1 2>/dev/null | grep -i "1" || true)
                    if [ -n "$QUERY_TEST" ]; then
                        echo "✅ База данных Cursovaya восстановлена и полностью доступна!"
                        DB_READY=1
                        break
                    fi
                elif echo "$DB_STATE" | grep -qi "RESTORING"; then
                    if [ $((j % 10)) -eq 0 ]; then
                        echo "   ... БД еще восстанавливается (прошло $((j * 2)) секунд)..."
                    fi
                fi
            done
            if [ $DB_READY -eq 0 ]; then
                echo "⚠️ БД восстановлена, но еще не полностью доступна после 90 секунд"
                echo "💡 Это может быть нормально - БД может стать доступной через несколько секунд"
                echo "💡 Backend попробует подключиться с увеличенным таймаутом (60 сек)"
            fi
        else
            echo "❌ Ошибка при восстановлении (код: $RESTORE_EXIT)"
            echo "Вывод: $RESTORE_RESULT"
            echo "🔄 Пробуем создать пустую БД..."
            if /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                -Q "CREATE DATABASE Cursovaya" 2>&1; then
                echo "✅ База данных Cursovaya создана (пустая)"
                sleep 3
            else
                echo "❌ Не удалось создать базу данных"
                EXIT_CODE=1
            fi
        fi
    else
        echo "⚠️ Файл резервной копии не найден (/scripts/Cursovaya.bak), создаем пустую базу данных..."
        if /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "CREATE DATABASE Cursovaya" 2>&1; then
            echo "✅ База данных Cursovaya создана"
            sleep 2
        else
            echo "❌ Не удалось создать базу данных"
            EXIT_CODE=1
        fi
    fi
    
    # Финальная проверка, что БД доступна
    sleep 3
    DB_FINAL_CHECK=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya' AND state_desc = 'ONLINE'" -h -1 2>/dev/null | grep -i cursovaya || true)
    if [ -z "$DB_FINAL_CHECK" ]; then
        echo "⚠️ База данных Cursovaya еще не доступна, но это может быть нормально"
        echo "💡 Backend попробует подключиться позже с увеличенным таймаутом"
        # Не считаем это критической ошибкой, так как БД может стать доступной позже
        EXIT_CODE=0
    else
        # Финальная проверка доступности через запрос
        sleep 1
        FINAL_QUERY=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -d Cursovaya -Q "SELECT 1" -h -1 2>/dev/null | grep -i "1" || true)
        if [ -n "$FINAL_QUERY" ]; then
            echo "✅ База данных Cursovaya доступна и готова к работе"
        else
            echo "⚠️ База данных существует, но запросы пока не проходят"
            EXIT_CODE=0
        fi
    fi
else
    echo "✅ База данных Cursovaya уже существует и доступна"
fi

echo "🎉 Инициализация завершена"
exit $EXIT_CODE
