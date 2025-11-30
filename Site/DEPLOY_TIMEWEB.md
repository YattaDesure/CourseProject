# Деплой на TimeWeb Cloud

## Вариант 1: Деплой через Docker (Рекомендуется)

### Подготовка

1. **Создайте Dockerfile для backend** (уже есть в `backend/Dockerfile`)
2. **Создайте Dockerfile для frontend** (уже есть в `frontend/Dockerfile`)
3. **Создайте docker-compose.yml** для production

### Шаги деплоя:

1. **Подключитесь к серверу TimeWeb Cloud по SSH**

2. **Клонируйте репозиторий:**
```bash
git clone <ваш-репозиторий> /var/www/greenquarter
cd /var/www/greenquarter
```

3. **Настройте переменные окружения:**
Создайте файл `.env` в корне проекта:
```env
# Database
DB_HOST=your-db-host.timeweb.ru
DB_PORT=1433
DB_NAME=Cursovaya
DB_USER=your_db_user
DB_PASSWORD=your_db_password

# JWT
JWT_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!
JWT_ISSUER=GreenQuarter
JWT_AUDIENCE=GreenQuarterUsers

# Frontend
VITE_API_URL=https://api.yourdomain.com

# CORS
ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

4. **Запустите через Docker Compose:**
```bash
docker compose -f docker-compose.prod.yml up -d --build
```

---

## Вариант 2: Прямой деплой без Docker

### Backend (ASP.NET Core)

1. **На сервере TimeWeb установите .NET 9 SDK:**
```bash
# Для Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0.408
```

2. **Соберите проект:**
```bash
cd /var/www/greenquarter/backend
dotnet publish -c Release -o /var/www/greenquarter-api
```

3. **Создайте systemd service** `/etc/systemd/system/greenquarter-api.service`:
```ini
[Unit]
Description=Green Quarter API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/greenquarter-api
ExecStart=/usr/bin/dotnet /var/www/greenquarter-api/GreenQuarter.Api.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5001
Environment=ConnectionStrings__DefaultConnection="Server=your-db-host;Database=Cursovaya;User Id=user;Password=pass;TrustServerCertificate=True;"
Environment=JWT__Key="YourSuperSecretKeyThatIsAtLeast32CharactersLongForProduction!"
Environment=JWT__Issuer="GreenQuarter"
Environment=JWT__Audience="GreenQuarterUsers"
Environment=AllowedOrigins="https://yourdomain.com"

[Install]
WantedBy=multi-user.target
```

4. **Запустите сервис:**
```bash
sudo systemctl enable greenquarter-api
sudo systemctl start greenquarter-api
```

### Frontend (Vue.js)

1. **Установите Node.js 20+ на сервере:**
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
```

2. **Соберите frontend:**
```bash
cd /var/www/greenquarter/frontend
npm install
npm run build
```

3. **Настройте Nginx** для статических файлов `/etc/nginx/sites-available/greenquarter`:
```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;

    root /var/www/greenquarter/frontend/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

4. **Активируйте конфигурацию:**
```bash
sudo ln -s /etc/nginx/sites-available/greenquarter /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Вариант 3: Использование панели TimeWeb

1. **В панели TimeWeb создайте:**
   - База данных SQL Server (или используйте существующую)
   - Домен для сайта (например: `greenquarter.yourdomain.com`)
   - Поддомен для API (например: `api.yourdomain.com`)

2. **Загрузите файлы через FTP/SFTP:**
   - Backend: загрузите собранные файлы из `backend/bin/Release/net9.0/publish/`
   - Frontend: загрузите собранные файлы из `frontend/dist/`

3. **Настройте переменные окружения в панели TimeWeb**

---

## Настройка базы данных

### Если используете SQL Server на TimeWeb:

1. **Создайте базу данных** через панель TimeWeb
2. **Импортируйте структуру** (если нужно):
   - Подключитесь к базе через SQL Server Management Studio
   - Или используйте существующую базу `Cursovaya`

### Если используете внешний SQL Server:

Обновите строку подключения в `appsettings.Production.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server.timeweb.ru,1433;Database=Cursovaya;User Id=user;Password=pass;TrustServerCertificate=True;Encrypt=True;"
}
```

---

## Настройка доменов

### Рекомендуемая структура:

- **Frontend:** `https://greenquarter.yourdomain.com` или `https://yourdomain.com`
- **Backend API:** `https://api.yourdomain.com` или `https://greenquarter.yourdomain.com/api`

### Обновите CORS в backend:

В `appsettings.Production.json` или переменных окружения:
```json
"AllowedOrigins": "https://yourdomain.com,https://www.yourdomain.com"
```

### Обновите API URL во frontend:

В `.env.production`:
```
VITE_API_URL=https://api.yourdomain.com
```

---

## Проверка после деплоя

1. **Проверьте API:**
```bash
curl https://api.yourdomain.com/api/auth/login
```

2. **Проверьте frontend:**
Откройте `https://yourdomain.com` в браузере

3. **Проверьте логи:**
```bash
# Backend
sudo journalctl -u greenquarter-api -f

# Nginx
sudo tail -f /var/log/nginx/error.log
```

---

## Обновление проекта

```bash
# На сервере
cd /var/www/greenquarter
git pull

# Backend
cd backend
dotnet publish -c Release -o /var/www/greenquarter-api
sudo systemctl restart greenquarter-api

# Frontend
cd ../frontend
npm install
npm run build
sudo systemctl reload nginx
```

---

## Безопасность

1. **Измените JWT ключ** на уникальный для production
2. **Используйте HTTPS** (настройте SSL в TimeWeb)
3. **Ограничьте CORS** только вашими доменами
4. **Используйте сильные пароли** для базы данных
5. **Настройте firewall** в панели TimeWeb

