# 🔧 Настройка деплоя на TimeWeb Cloud

## Проблема с Dockerfile

TimeWeb Cloud пытается собрать проект из корня, но `package.json` находится в папке `frontend/`.

## Решение

### Вариант 1: Использовать Dockerfile в корне (Рекомендуется)

Создан файл `Dockerfile` в корне проекта, который правильно собирает frontend.

**В настройках TimeWeb:**
1. Убедитесь, что используется Dockerfile из корня проекта
2. Build context: корень проекта (`.`)
3. Dockerfile path: `Dockerfile`

### Вариант 2: Настроить Build Context на frontend/

**В настройках TimeWeb:**
1. Build context: `frontend/`
2. Dockerfile path: `frontend/Dockerfile`

---

## Настройка в панели TimeWeb

### Для Frontend:

1. **Перейдите в настройки приложения**
2. **Build Settings:**
   - Build context: `.` (корень проекта) или `frontend/`
   - Dockerfile: `Dockerfile` (если в корне) или `frontend/Dockerfile`
   - Port: `80`

3. **Environment Variables:**
   ```
   VITE_API_URL=https://api.вашдомен.com
   ```

### Для Backend (если деплоите отдельно):

1. **Build Settings:**
   - Build context: `backend/`
   - Dockerfile: `backend/Dockerfile`
   - Port: `8080` или `5001`

2. **Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ConnectionStrings__DefaultConnection=Server=0.tcp.ngrok.io,12345;Database=Cursovaya;User Id=SA;Password=22332123Yaz;TrustServerCertificate=True;Encrypt=True;
   JWT__Key=ВашСекретныйКлючМинимум32СимволаДляПродакшена!
   JWT__Issuer=GreenQuarter
   JWT__Audience=GreenQuarterUsers
   AllowedOrigins=https://вашдомен.com
   ```

---

## Структура проекта

```
/
├── Dockerfile              ← Для frontend (TimeWeb)
├── frontend/
│   ├── Dockerfile         ← Альтернативный (если build context = frontend/)
│   ├── package.json
│   └── ...
├── backend/
│   ├── Dockerfile
│   └── ...
└── docker-compose.prod.yml
```

---

## Проверка после деплоя

1. **Проверьте логи сборки** в панели TimeWeb
2. **Проверьте, что контейнер запущен:**
   ```bash
   docker ps
   ```
3. **Откройте сайт** в браузере

---

## Если ошибка повторяется

1. **Убедитесь, что `Dockerfile` в корне проекта** (создан автоматически)
2. **Проверьте build context** в настройках TimeWeb
3. **Попробуйте изменить build context на `frontend/`** и использовать `frontend/Dockerfile`

---

## Альтернатива: Ручной деплой

Если автоматический деплой не работает, можно задеплоить вручную:

1. **Соберите frontend локально:**
   ```bash
   cd frontend
   npm install
   npm run build
   ```

2. **Загрузите папку `dist/` на сервер** через FTP/SFTP

3. **Настройте Nginx** для статических файлов (см. DEPLOY_QUICKSTART.md)

