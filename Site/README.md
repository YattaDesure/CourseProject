# Green Quarter Property Management

Система управления недвижимостью для ЖК "Green Quarter".

## 🚀 Быстрый старт

### Требования:
- .NET 9 SDK
- Node.js 20+
- SQL Server (локально или в Docker)

### Запуск Backend:

```bash
cd backend
dotnet run --project GreenQuarter.Api
```

Backend будет доступен на: `http://localhost:5001`

### Запуск Frontend:

```bash
cd frontend
npm install
npm run dev
```

Frontend будет доступен на: `http://localhost:5173`

## 📋 Настройка

### База данных:

Настройки подключения в `backend/GreenQuarter.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=GreenQuarter;User Id=sa;Password=ваш_пароль;TrustServerCertificate=True;"
}
```

### JWT:

Настройки JWT в `backend/GreenQuarter.Api/appsettings.json`:

```json
"JWT": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "GreenQuarter",
  "Audience": "GreenQuarterUsers"
}
```

## 🔐 Вход в систему

- Email: `edikyazikov1@gmail.com`
- Password: `123456`

Или используйте другие тестовые аккаунты:
- Email: `motylkova@gmail.com`, Password: `admin123` (Admin)
- Email: `annayazykova@gmail.com`, Password: `moderator123` (Moderator)

## 📁 Структура проекта

- `backend/` - ASP.NET Core API
- `frontend/` - Vue.js 3 приложение
- `source/` - Иконки и изображения

## 🛠️ Разработка

### Backend:
- Swagger UI: `http://localhost:5001/swagger`

### Frontend:
- Dev сервер: `http://localhost:5173`
- API проксируется через Vite на `http://localhost:5001`

---

**Готово к разработке! 🎉**
