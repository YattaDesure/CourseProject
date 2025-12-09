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
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
    -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya' AND state_desc = 'ONLINE'" -h -1 2>/dev/null | grep -i cursovaya || true)

if [ -z "$DB_EXISTS" ]; then
    echo "📦 База данных не найдена, создаем/восстанавливаем..."
    
    # Удаляем БД, если она существует в неправильном состоянии
    /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
        -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Cursovaya') BEGIN ALTER DATABASE Cursovaya SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE Cursovaya; END" 2>&1 || true
    sleep 2
    
    # Проверяем наличие файла резервной копии
    if [ -f "/scripts/Cursovaya.bak" ]; then
        echo "📥 Найден файл резервной копии: /scripts/Cursovaya.bak"
        echo "🔄 Восстановление базы данных из резервной копии..."
        
        # Восстанавливаем БД - используем путь, доступный из SQL Server контейнера
        # SQL Server видит файл по пути /backup (смонтирован в docker-compose)
        RESTORE_OUTPUT=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "RESTORE DATABASE Cursovaya FROM DISK = '/backup/Cursovaya.bak' WITH REPLACE" 2>&1)
        RESTORE_EXIT=$?
        
        if [ $RESTORE_EXIT -eq 0 ]; then
            echo "✅ Команда RESTORE выполнена"
            # Ждем, пока БД станет доступной и таблицы будут готовы
            echo "⏳ Ожидание доступности базы данных и таблиц..."
            DB_READY=0
            for j in {1..40}; do
                sleep 2
                # Проверяем, что БД ONLINE и доступна для запросов
                DB_TEST=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                    -d Cursovaya -Q "SELECT 1" -h -1 2>/dev/null | grep -i "1" || true)
                if [ -n "$DB_TEST" ]; then
                    # Дополнительная проверка - есть ли таблица Residents и можем ли мы к ней обратиться
                    TABLE_CHECK=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                        -d Cursovaya -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Residents'" -h -1 -W 2>/dev/null | grep -E "^[[:space:]]*1[[:space:]]*$" || true)
                    if [ -n "$TABLE_CHECK" ]; then
                        # Проверяем таблицы Identity (AspNetUsers, AspNetRoles и т.д.)
                        IDENTITY_TABLES=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                            -d Cursovaya -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'AspNet%'" -h -1 -W 2>/dev/null | grep -E "^[[:space:]]*[0-9]+[[:space:]]*$" || true)
                        if [ -n "$IDENTITY_TABLES" ] && [ "$IDENTITY_TABLES" -gt "0" ]; then
                            # Финальная проверка - можем ли мы выполнить запрос к таблице
                            sleep 1
                            DATA_CHECK=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                                -d Cursovaya -Q "SELECT COUNT(*) FROM Residents" -h -1 -W 2>/dev/null | grep -E "^[[:space:]]*[0-9]+[[:space:]]*$" || true)
                            if [ -n "$DATA_CHECK" ]; then
                                # Проверяем AspNetUsers
                                ASPNET_CHECK=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                                    -d Cursovaya -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers'" -h -1 -W 2>/dev/null | grep -E "^[[:space:]]*1[[:space:]]*$" || true)
                                if [ -n "$ASPNET_CHECK" ]; then
                                    echo "✅ База данных Cursovaya восстановлена, все таблицы готовы, данные доступны!"
                                    DB_READY=1
                                    break
                                fi
                            fi
                        fi
                    fi
                fi
                if [ $((j % 10)) -eq 0 ]; then
                    echo "   ... еще ждем (прошло $((j * 2)) секунд)..."
                fi
            done
            if [ $DB_READY -eq 0 ]; then
                echo "⚠️ БД восстановлена, но таблицы еще не готовы после 80 секунд"
                echo "💡 Backend попробует подключиться с таймаутом 60 секунд"
            fi
        else
            echo "❌ Ошибка при восстановлении:"
            echo "$RESTORE_OUTPUT"
            echo "🔄 Создаем пустую БД..."
            /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                -Q "CREATE DATABASE Cursovaya" 2>&1
            sleep 2
        fi
    else
        echo "⚠️ Файл резервной копии не найден, создаем пустую базу данных..."
        /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "CREATE DATABASE Cursovaya" 2>&1
        sleep 2
    fi
    
    # Финальная проверка доступности БД
    sleep 2
    FINAL_TEST=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
        -d Cursovaya -Q "SELECT 1" -h -1 2>/dev/null | grep -i "1" || true)
    if [ -n "$FINAL_TEST" ]; then
        echo "✅ База данных Cursovaya готова к работе"
    else
        echo "⚠️ База данных создана, но запросы пока не проходят"
        echo "💡 Backend попробует подключиться с таймаутом 60 секунд"
    fi
else
    echo "✅ База данных Cursovaya уже существует"
fi

echo "🎉 Инициализация завершена"
exit 0
