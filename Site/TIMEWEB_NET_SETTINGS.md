# ⚙️ Настройки для .NET деплоя на TimeWeb

## 📋 Что заполнить в форме TimeWeb:

### 1. Окружение (Environment)
✅ **Оставьте:** `.NET` (уже правильно)

### 2. Фреймворк (Framework)
✅ **Оставьте:** `ASP.NET` (уже правильно)

### 3. Версия окружения (Environment Version)
✅ **Оставьте:** `9.0` (уже правильно)

### 4. Команда сборки (Build command)
**Измените на:**
```
dotnet build backend/GreenQuarter.Api/GreenQuarter.Api.csproj -c Release
```

**Или если Build context = `backend/`:**
```
dotnet build GreenQuarter.Api/GreenQuarter.Api.csproj -c Release
```

**Или более простой вариант:**
```
cd backend && dotnet build GreenQuarter.Api/GreenQuarter.Api.csproj -c Release
```

### 5. Зависимости (Dependencies)
**Оставьте пустым** или укажите системные зависимости, если нужны:
```
(пусто)
```

### 6. Команда запуска (Run command)
**Измените на:**
```
dotnet run --project backend/GreenQuarter.Api/GreenQuarter.Api.csproj --urls "http://0.0.0.0:8080"
```

**Или если Build context = `backend/`:**
```
dotnet run --project GreenQuarter.Api/GreenQuarter.Api.csproj --urls "http://0.0.0.0:8080"
```

**Или:**
```
cd backend/GreenQuarter.Api && dotnet run --urls "http://0.0.0.0:8080"
```

### 7. Путь проверки состояния (Health check path)
**Можно указать:**
```
/swagger
```
Или оставьте пустым.

---

## 🎯 Рекомендуемые настройки:

### Если Build context = `.` (корень проекта):

**Команда сборки:**
```
dotnet build backend/GreenQuarter.Api/GreenQuarter.Api.csproj -c Release
```

**Команда запуска:**
```
dotnet run --project backend/GreenQuarter.Api/GreenQuarter.Api.csproj --urls "http://0.0.0.0:8080"
```

### Если Build context = `backend/`:

**Команда сборки:**
```
dotnet build GreenQuarter.Api/GreenQuarter.Api.csproj -c Release
```

**Команда запуска:**
```
dotnet run --project GreenQuarter.Api/GreenQuarter.Api.csproj --urls "http://0.0.0.0:8080"
```

---

## ⚠️ Важно:

1. **Порт:** Убедитесь, что приложение слушает на `0.0.0.0:8080` (не `localhost`)
2. **Переменные окружения:** Не забудьте добавить все переменные окружения (DB_HOST, DB_PORT, JWT_KEY и т.д.)
3. **Build context:** Проверьте, какой Build context указан в настройках приложения

---

## 🔄 Альтернатива: Использовать Docker

Если нативные команды .NET не работают, можно переключиться на Docker:

1. В настройках приложения найдите опцию "Тип сборки" или "Build type"
2. Выберите **"Docker"** вместо **".NET"**
3. Укажите:
   - Build context: `backend/`
   - Dockerfile: `Dockerfile`

Это будет использовать наш обновленный Dockerfile.

