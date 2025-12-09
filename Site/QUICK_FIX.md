# Быстрое решение проблем на другом устройстве

## Если видите ошибку "Invalid object name 'AspNetUsers'" или "Login failed for user 'sa'"

### Шаг 1: Полная очистка и перезапуск

```bash
# Остановите все и удалите volumes
docker compose down -v

# Убедитесь, что файл backup существует
ls -lh init-db/Cursovaya.bak

# Если файла нет - скопируйте его в папку init-db/
```

### Шаг 2: Запуск с увеличенным временем ожидания

```bash
# Запустите все контейнеры
docker compose up -d

# Подождите 3-5 минут (БД восстанавливается медленно)
# Следите за логами:
docker compose logs -f db-init
```

### Шаг 3: Проверка готовности БД

После того как db-init завершится, проверьте:

```bash
# Проверка БД
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -Q "SELECT name FROM sys.databases WHERE name = 'Cursovaya'"

# Проверка таблиц Identity
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -d Cursovaya -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'AspNet%'"

# Проверка данных
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -d Cursovaya -Q "SELECT COUNT(*) FROM Residents"
```

### Шаг 4: Если таблицы Identity отсутствуют

Если таблицы `AspNetUsers` нет, нужно запустить миграции:

```bash
# Войдите в контейнер backend
docker exec -it greenquarter-api bash

# Запустите миграции (если есть команда)
# Или перезапустите backend - он создаст таблицы при первом запуске
docker compose restart backend
```

### Шаг 5: Проверка работы API

```bash
# Проверка Swagger
curl http://localhost:5001/swagger/index.html

# Проверка авторизации
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"edikyazikov1@gmail.com","password":"123456"}'
```

## Если проблема на Windows

### Проблема с окончаниями строк

Если видите `$'\r': command not found`:

```bash
# В Git Bash или WSL
dos2unix init-db/init-database.sh

# Или пересохраните файл в редакторе с LF окончаниями
```

### Проблема с путями

Убедитесь, что используете правильные пути:
- В Git Bash: `./init-db/Cursovaya.bak`
- В PowerShell: `.\init-db\Cursovaya.bak`

## Если проблема на Linux

### Проблема с правами

```bash
chmod +x init-db/init-database.sh
chmod 644 init-db/Cursovaya.bak
```

## Альтернативное решение: Ручная инициализация

Если автоматическая инициализация не работает:

```bash
# 1. Запустите только SQL Server
docker compose up -d sqlserver

# 2. Подождите 30 секунд
sleep 30

# 3. Вручную восстановите БД
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -Q "RESTORE DATABASE Cursovaya FROM DISK = '/backup/Cursovaya.bak' WITH REPLACE"

# 4. Подождите 30 секунд
sleep 30

# 5. Проверьте таблицы
docker exec greenquarter-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "22332123Yaz" -C \
  -d Cursovaya -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"

# 6. Запустите backend
docker compose up -d backend frontend
```

## Контрольный список

- [ ] Файл `init-db/Cursovaya.bak` существует (размер ~5.6MB)
- [ ] SQL Server контейнер запущен и healthy
- [ ] db-init контейнер завершился успешно
- [ ] БД `Cursovaya` существует
- [ ] Таблицы `AspNetUsers` и `Residents` существуют
- [ ] Backend контейнер запущен (может быть unhealthy первые минуты)
- [ ] API отвечает на запросы

## Если ничего не помогает

1. Удалите все: `docker compose down -v`
2. Удалите образы: `docker rmi site-backend site-frontend`
3. Пересоберите: `docker compose build --no-cache`
4. Запустите: `docker compose up -d`
5. Подождите 5 минут
6. Проверьте логи: `docker compose logs`

