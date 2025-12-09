#!/bin/bash
echo "🚀 Запуск Green Quarter через Docker Compose..."
echo ""
docker compose up -d
echo ""
echo "⏳ Ожидание запуска контейнеров..."
sleep 10

# Инициализация базы данных
if [ -f "./init-db.sh" ]; then
    echo "📦 Инициализация базы данных..."
    ./init-db.sh
fi

echo ""
echo "✅ Запущено!"
echo ""
echo "🌐 Доступно:"
echo "  Frontend: http://localhost"
echo "  Backend:  http://localhost:5001"
echo "  Swagger:  http://localhost:5001/swagger"
echo "  SQL Server: localhost:1433"
echo ""
echo "📋 Просмотр логов: docker compose logs -f"
echo "🛑 Остановка: docker compose down"
