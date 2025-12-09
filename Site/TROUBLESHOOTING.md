# Устранение проблем при запуске на разных устройствах

## Проблема: "Login failed for user 'sa'" или "Cannot open database"

### Решение 1: Полная переустановка

```bash
# Остановите все контейнеры и удалите volumes
docker compose down -v

# Убедитесь, что файл backup существует
ls -lh init-db/Cursovaya.bak

# Запустите заново
docker compose up -d

# Подождите 2-3 минуты и проверьте логи
docker compose logs db-init
docker compose logs backend
```

### Решение 2: Проверка файла backup

Убедитесь, что файл `init-db/Cursovaya.bak` существует и имеет размер около 5.6MB:

```bash
ls -lh init-db/Cursovaya.bak
```

Если файла нет, скопируйте его в папку `init-db/`.

### Решение 3: Ручная инициализация БД

Если автоматическая инициализация не работает:

```bash
# Запустите только SQL Server
docker compose up -d sqlserver

# Подождите 30 секунд
sleep 30

# Вручную восстановите БД
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -Q "RESTORE DATABASE Cursovaya FROM DISK = '/backup/Cursovaya.bak' WITH REPLACE"

# Запустите остальные контейнеры
docker compose up -d
```

### Решение 4: Проверка окончаний строк (Windows)

Если вы на Windows и видите ошибки `$'\r': command not found`:

```bash
# В Git Bash или WSL
dos2unix init-db/init-database.sh

# Или вручную пересохраните файл с LF окончаниями
```

### Решение 5: Проверка прав доступа (Linux)

На Linux могут быть проблемы с правами:

```bash
chmod +x init-db/init-database.sh
chmod 644 init-db/Cursovaya.bak
```

### Решение 6: Проверка подключения к БД

Проверьте, что БД действительно создана:

```bash
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya'"
```

Проверьте таблицы:

```bash
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -d Cursovaya -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"
```

### Решение 7: Увеличение таймаутов

Если БД восстанавливается медленно, увеличьте задержку в `docker-compose.yml`:

```yaml
entrypoint: >
  /bin/sh -c "
  echo '⏳ Ожидание готовности базы данных Cursovaya (60 секунд)...';
  sleep 60;
  echo '🚀 Запуск backend...';
  dotnet GreenQuarter.Api.dll
  "
```

## Частые ошибки

### "Invalid object name 'Residents'"
- **Причина**: БД восстановилась, но таблицы еще не готовы
- **Решение**: Увеличьте задержку в backend entrypoint до 60 секунд

### "Cannot open database 'Cursovaya'"
- **Причина**: БД не была создана/восстановлена
- **Решение**: Проверьте логи `db-init`, убедитесь что backup файл существует

### "Login failed for user 'sa'"
- **Причина**: Backend пытается подключиться до готовности БД
- **Решение**: Увеличьте задержку в backend entrypoint

## Проверка работоспособности

После запуска проверьте:

```bash
# 1. Все контейнеры запущены
docker compose ps

# 2. БД существует и содержит данные
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -d Cursovaya -Q "SELECT COUNT(*) FROM Residents"

# 3. API отвечает
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"edikyazikov1@gmail.com","password":"123456"}'
```

## Если ничего не помогает

1. Удалите все volumes: `docker compose down -v`
2. Убедитесь, что файл `init-db/Cursovaya.bak` существует
3. Запустите заново: `docker compose up -d`
4. Подождите 3-5 минут
5. Проверьте логи: `docker compose logs`

