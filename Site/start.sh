#!/bin/bash
echo "🚀 Запуск Green Quarter через Docker Compose..."
echo ""
docker compose up -d
echo ""
echo "✅ Запущено!"
echo ""
echo "🌐 Доступно:"
echo "  Frontend: http://localhost"
echo "  Backend:  http://localhost:5001"
echo "  Swagger:  http://localhost:5001/swagger"
echo ""
echo "📋 Просмотр логов: docker compose logs -f"
echo "🛑 Остановка: docker compose down"
