# 🚀 Быстрый деплой на TimeWeb Cloud

> ⚠️ **ВАЖНО:** Если база данных в Docker на вашем ноутбуке, сначала прочитайте **DEPLOY_DOCKER_DB.md** для настройки туннеля!

## Шаг 1: Подготовка на локальной машине

```bash
# 1. Соберите frontend для production
cd frontend
npm install
npm run build

# 2. Соберите backend для production
cd ../backend
dotnet publish -c Release -o ./publish
```

## Шаг 2: Загрузка на сервер TimeWeb

### Вариант A: Через Git (рекомендуется)

```bash
# На сервере TimeWeb
git clone <ваш-репозиторий> /var/www/greenquarter
cd /var/www/greenquarter
```

### Вариант B: Через FTP/SFTP

Загрузите папки:
- `backend/publish/` → `/var/www/greenquarter-api/`
- `frontend/dist/` → `/var/www/greenquarter-frontend/`

## Шаг 3: Настройка на сервере

### 1. Создайте файл `.env` в корне проекта:

```bash
nano /var/www/greenquarter/.env
```

Содержимое:
```env
# Если БД на ноутбуке - используйте адрес из ngrok/cloudflared
# Например: DB_HOST=0.tcp.ngrok.io, DB_PORT=12345
# Если БД на TimeWeb - используйте хост TimeWeb
DB_HOST=ваш-хост-базы.timeweb.ru
DB_PORT=1433
DB_NAME=Cursovaya
DB_USER=ваш_пользователь
DB_PASSWORD=ваш_пароль

JWT_KEY=ВашСекретныйКлючМинимум32СимволаДляПродакшена!
JWT_ISSUER=GreenQuarter
JWT_AUDIENCE=GreenQuarterUsers

VITE_API_URL=https://api.вашдомен.com
ALLOWED_ORIGINS=https://вашдомен.com,https://www.вашдомен.com
```

### 2. Установите .NET 9 на сервере:

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0.408
export PATH=$PATH:$HOME/.dotnet
```

### 3. Создайте systemd сервис для API:

```bash
sudo nano /etc/systemd/system/greenquarter-api.service
```

Вставьте:
```ini
[Unit]
Description=Green Quarter API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/greenquarter/backend
ExecStart=/root/.dotnet/dotnet /var/www/greenquarter/backend/GreenQuarter.Api.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5001
Environment=ConnectionStrings__DefaultConnection="Server=ваш-хост;Database=Cursovaya;User Id=ваш_пользователь;Password=ваш_пароль;TrustServerCertificate=True;"
Environment=JWT__Key="ВашСекретныйКлючМинимум32СимволаДляПродакшена!"
Environment=JWT__Issuer="GreenQuarter"
Environment=JWT__Audience="GreenQuarterUsers"
Environment=AllowedOrigins="https://вашдомен.com"

[Install]
WantedBy=multi-user.target
```

Запустите:
```bash
sudo systemctl daemon-reload
sudo systemctl enable greenquarter-api
sudo systemctl start greenquarter-api
sudo systemctl status greenquarter-api
```

### 4. Настройте Nginx для frontend:

```bash
sudo nano /etc/nginx/sites-available/greenquarter
```

Вставьте:
```nginx
server {
    listen 80;
    server_name вашдомен.com www.вашдомен.com;

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

Активируйте:
```bash
sudo ln -s /etc/nginx/sites-available/greenquarter /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### 5. Настройте SSL (HTTPS) в панели TimeWeb

Включите SSL для вашего домена через панель управления TimeWeb.

## Шаг 4: Проверка

1. Откройте `https://вашдомен.com` в браузере
2. Проверьте логи API: `sudo journalctl -u greenquarter-api -f`
3. Проверьте логи Nginx: `sudo tail -f /var/log/nginx/error.log`

## Обновление проекта

```bash
cd /var/www/greenquarter
git pull

# Backend
cd backend
dotnet publish -c Release
sudo systemctl restart greenquarter-api

# Frontend
cd ../frontend
npm install
npm run build
sudo systemctl reload nginx
```

---

## Альтернатива: Docker Compose

Если на сервере установлен Docker:

```bash
cd /var/www/greenquarter
cp .env.production.example .env
# Отредактируйте .env
docker compose -f docker-compose.prod.yml up -d --build
```

---

## Проблемы?

- **API не запускается:** Проверьте логи `sudo journalctl -u greenquarter-api -n 50`
- **502 Bad Gateway:** Убедитесь, что API запущен на порту 5001
- **CORS ошибки:** Проверьте `AllowedOrigins` в настройках
- **База данных не подключается:** Проверьте строку подключения и firewall

