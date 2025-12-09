#!/bin/bash
echo "🚀 Запуск Green Quarter через Docker Compose..."
echo ""
echo "📦 База данных будет автоматически инициализирована при первом запуске"
echo ""
docker compose up -d
echo ""
echo "⏳ Ожидание запуска контейнеров..."
sleep 15

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
