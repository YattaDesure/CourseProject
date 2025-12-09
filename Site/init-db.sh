#!/bin/bash

# Скрипт инициализации базы данных
# Запускается после старта контейнеров

echo "🔍 Проверка базы данных Cursovaya..."

# Ждем, пока SQL Server будет готов
echo "⏳ Ожидание готовности SQL Server..."
until docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "22332123Yaz" -C -Q "SELECT 1" &> /dev/null
do
    sleep 2
done

echo "✅ SQL Server готов"

# Проверяем, существует ли база данных
DB_EXISTS=$(docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "22332123Yaz" -C -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya'" -h -1 2>/dev/null | grep -i cursovaya)

if [ -z "$DB_EXISTS" ]; then
    echo "📦 База данных не найдена, восстанавливаем из резервной копии..."
    
    if [ -f "./init-db/Cursovaya.bak" ]; then
        echo "📥 Найден файл резервной копии"
        
        # Восстанавливаем базу данных из смонтированного файла
        echo "🔄 Восстановление базы данных..."
        docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U SA \
            -P "22332123Yaz" \
            -C \
            -Q "RESTORE DATABASE Cursovaya FROM DISK = '/backup/Cursovaya.bak' WITH REPLACE"
        
        if [ $? -eq 0 ]; then
            echo "✅ База данных Cursovaya успешно восстановлена!"
        else
            echo "❌ Ошибка при восстановлении, создаем пустую БД"
            docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
                -S localhost \
                -U SA \
                -P "22332123Yaz" \
                -C \
                -Q "CREATE DATABASE Cursovaya"
            echo "✅ База данных Cursovaya создана (пустая)"
        fi
    else
        echo "⚠️ Файл резервной копии не найден, создаем пустую базу данных..."
        docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U SA \
            -P "22332123Yaz" \
            -C \
            -Q "CREATE DATABASE Cursovaya"
        echo "✅ База данных Cursovaya создана"
    fi
else
    echo "✅ База данных Cursovaya уже существует"
fi

echo "🎉 Инициализация завершена"

