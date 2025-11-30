# 🚀 Деплой на TimeWeb Cloud с базой данных в Docker на ноутбуке

> 💡 **Важно:** База данных SQL Server запущена в Docker на вашем ноутбуке. Вы сами управляете её запуском.

## Вариант 1: Использование ngrok (Рекомендуется - самый простой)

### Шаг 1: Убедитесь, что Docker контейнер с БД запущен

На вашем ноутбуке проверьте, что SQL Server в Docker работает:

```bash
docker ps | grep sql
# Или
docker ps
```

Если контейнер не запущен, запустите его (команда зависит от вашей конфигурации):
```bash
# Пример, если используете docker-compose:
docker-compose up -d sqlserver

# Или если контейнер называется иначе:
docker start <имя-контейнера>
```

Убедитесь, что порт 1433 проброшен на хост (обычно `-p 1433:1433`).

### Шаг 2: Установите ngrok на ноутбук

```bash
# macOS
brew install ngrok

# Или скачайте с https://ngrok.com/download
```

### Шаг 3: Зарегистрируйтесь на ngrok.com и получите токен

1. Зарегистрируйтесь на https://ngrok.com (бесплатный план)
2. Скопируйте токен авторизации
3. Авторизуйтесь:
```bash
ngrok config add-authtoken <ваш-токен>
```

### Шаг 4: Создайте туннель для SQL Server

На вашем ноутбуке запустите:

```bash
ngrok tcp 1433
```

Вы увидите что-то вроде:
```
Forwarding  tcp://0.tcp.ngrok.io:12345 -> localhost:1433
```

**Сохраните адрес** `0.tcp.ngrok.io` и **порт** `12345` (они будут другими).

### Шаг 5: Настройте сервер TimeWeb

В строке подключения на сервере используйте адрес ngrok:

```bash
# На сервере TimeWeb
sudo nano /etc/systemd/system/greenquarter-api.service
```

Обновите строку подключения:
```ini
Environment=ConnectionStrings__DefaultConnection="Server=0.tcp.ngrok.io,12345;Database=Cursovaya;User Id=SA;Password=22332123Yaz;TrustServerCertificate=True;Encrypt=True;"
```

**Важно:** Адрес ngrok меняется при каждом перезапуске! Для постоянного адреса нужен платный план ngrok или используйте Вариант 2.

---

## Вариант 2: Использование cloudflared (Бесплатно, постоянный адрес)

### Шаг 1: Установите cloudflared на ноутбук

```bash
# macOS
brew install cloudflared

# Или скачайте с https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/install-and-setup/installation/
```

### Шаг 2: Создайте туннель

```bash
# Создайте туннель (первый раз)
cloudflared tunnel create greenquarter-db

# Запустите туннель
cloudflared tunnel --url tcp://localhost:1433
```

Вы получите постоянный адрес вида: `tcp://xxxxx.trycloudflare.com:xxxxx`

### Шаг 3: Используйте адрес в настройках сервера

Обновите строку подключения на сервере TimeWeb с адресом cloudflared.

---

## Вариант 3: SSH туннель (Если есть SSH доступ к серверу)

### Шаг 1: Настройте SSH туннель на сервере

На сервере TimeWeb создайте systemd service для SSH туннеля:

```bash
sudo nano /etc/systemd/system/db-tunnel.service
```

```ini
[Unit]
Description=SSH Tunnel to Local Database
After=network.target

[Service]
Type=simple
User=root
ExecStart=/usr/bin/ssh -N -L 127.0.0.1:1433:localhost:1433 -o ServerAliveInterval=60 -o ServerAliveCountMax=3 ваш_пользователь@ваш_ноутбук_ip
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**Проблема:** Нужен статический IP ноутбука или динамический DNS.

---

## Вариант 4: Прямое подключение (Если есть статический IP)

Если у вашего ноутбука есть статический внешний IP:

### Шаг 1: Узнайте внешний IP ноутбука

```bash
curl ifconfig.me
```

### Шаг 2: Настройте проброс портов на роутере

- Зайдите в настройки роутера
- Настройте Port Forwarding: внешний порт 1433 → внутренний IP ноутбука:1433

### Шаг 3: Используйте IP в строке подключения

```ini
Environment=ConnectionStrings__DefaultConnection="Server=ваш_внешний_ip,1433;Database=Cursovaya;User Id=SA;Password=22332123Yaz;TrustServerCertificate=True;Encrypt=True;"
```

---

## Рекомендуемая настройка (ngrok с автозапуском)

### Создайте скрипт для автозапуска ngrok на ноутбуке

Создайте файл `start-db-tunnel.sh` на ноутбуке:

```bash
#!/bin/bash

# Получите адрес ngrok
NGROK_URL=$(ngrok tcp 1433 --log=stdout 2>&1 | grep -oP 'tcp://\K[^:]+' | head -1)
NGROK_PORT=$(ngrok tcp 1433 --log=stdout 2>&1 | grep -oP 'tcp://[^:]+:\K[0-9]+' | head -1)

echo "Туннель создан: $NGROK_URL:$NGROK_PORT"
echo "Обновите строку подключения на сервере:"
echo "Server=$NGROK_URL,$NGROK_PORT;Database=Cursovaya;User Id=SA;Password=22332123Yaz;TrustServerCertificate=True;Encrypt=True;"

# Держите туннель открытым
ngrok tcp 1433
```

Сделайте исполняемым:
```bash
chmod +x start-db-tunnel.sh
```

### Автозапуск при загрузке (macOS)

Создайте LaunchAgent:

```bash
nano ~/Library/LaunchAgents/com.greenquarter.ngrok.plist
```

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.greenquarter.ngrok</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/ngrok</string>
        <string>tcp</string>
        <string>1433</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>
```

Загрузите:
```bash
launchctl load ~/Library/LaunchAgents/com.greenquarter.ngrok.plist
```

---

## Настройка на сервере TimeWeb

### Обновленный systemd service

```bash
sudo nano /etc/systemd/system/greenquarter-api.service
```

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
# ЗАМЕНИТЕ на адрес из ngrok/cloudflared
Environment=ConnectionStrings__DefaultConnection="Server=0.tcp.ngrok.io,12345;Database=Cursovaya;User Id=SA;Password=22332123Yaz;TrustServerCertificate=True;Encrypt=True;"
Environment=JWT__Key="ВашСекретныйКлючМинимум32СимволаДляПродакшена!"
Environment=JWT__Issuer="GreenQuarter"
Environment=JWT__Audience="GreenQuarterUsers"
Environment=AllowedOrigins="https://вашдомен.com"

[Install]
WantedBy=multi-user.target
```

---

## Проверка подключения

### На сервере TimeWeb проверьте подключение:

```bash
# Установите sqlcmd (если нужно)
# Затем проверьте:
sqlcmd -S 0.tcp.ngrok.io,12345 -U SA -P 22332123Yaz -C -Q "SELECT @@VERSION"
```

---

## Важные замечания

1. **Безопасность:**
   - Используйте сильный пароль для SQL Server
   - Ограничьте доступ только с IP сервера TimeWeb (если возможно)
   - Рассмотрите использование VPN вместо публичного туннеля

2. **Производительность:**
   - Туннель добавляет задержку
   - Для production лучше использовать облачную БД

3. **Надежность:**
   - Убедитесь, что туннель всегда запущен
   - Настройте мониторинг и автоперезапуск

4. **Бесплатные туннели:**
   - ngrok: бесплатный план имеет ограничения
   - cloudflared: полностью бесплатный, но адрес может меняться
   - Для постоянного адреса нужен платный план

---

## Быстрый старт (ngrok)

1. **На ноутбуке:**
   ```bash
   ngrok tcp 1433
   # Скопируйте адрес (например: 0.tcp.ngrok.io:12345)
   ```

2. **На сервере TimeWeb:**
   - Обновите строку подключения в systemd service
   - Перезапустите сервис: `sudo systemctl restart greenquarter-api`

3. **Проверьте:**
   - Откройте сайт и попробуйте войти

---

## Альтернатива: Локальная разработка

Если деплой на TimeWeb слишком сложен, можно:
- Разрабатывать локально
- Использовать ngrok для frontend (чтобы показать клиенту)
- База данных остается на ноутбуке

