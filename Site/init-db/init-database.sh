#!/bin/bash
set -e

# Скрипт инициализации базы данных
# Запускается после готовности SQL Server

SA_PASSWORD="${SA_PASSWORD:-22332123Yaz}"
SQL_SERVER="${SQL_SERVER:-sqlserver}"

echo "⏳ Ожидание готовности SQL Server..."

# Ждем, пока SQL Server будет готов принимать подключения
for i in {1..30}; do
    if /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null; then
        echo "✅ SQL Server готов"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ SQL Server не готов после 30 попыток"
        exit 1
    fi
    sleep 2
done

echo "🔍 Проверка базы данных Cursovaya..."

# Проверяем, существует ли база данных
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya'" -h -1 2>/dev/null | grep -i cursovaya || true)

if [ -z "$DB_EXISTS" ]; then
    echo "📦 База данных не найдена, восстанавливаем из резервной копии..."
    
    # Проверяем наличие файла резервной копии
    if [ -f "/scripts/Cursovaya.bak" ]; then
        echo "📥 Найден файл резервной копии: /scripts/Cursovaya.bak"
        echo "🔄 Восстановление базы данных..."
        
        /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "RESTORE DATABASE Cursovaya FROM DISK = '/scripts/Cursovaya.bak' WITH REPLACE"
        
        if [ $? -eq 0 ]; then
            echo "✅ База данных Cursovaya успешно восстановлена!"
        else
            echo "❌ Ошибка при восстановлении, создаем пустую БД"
            /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
                -Q "CREATE DATABASE Cursovaya"
            echo "✅ База данных Cursovaya создана (пустая)"
        fi
    else
        echo "⚠️ Файл резервной копии не найден (/scripts/Cursovaya.bak), создаем пустую базу данных..."
        /opt/mssql-tools18/bin/sqlcmd -S $SQL_SERVER -U SA -P "$SA_PASSWORD" -C \
            -Q "CREATE DATABASE Cursovaya"
        echo "✅ База данных Cursovaya создана"
    fi
else
    echo "✅ База данных Cursovaya уже существует"
fi

echo "🎉 Инициализация завершена"
exit 0

