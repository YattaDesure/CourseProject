FROM mcr.microsoft.com/mssql/server:2025-CTP2.0-ubuntu-22.04

# Копируем скрипт инициализации
COPY init-database.sh /init-database.sh
RUN chmod +x /init-database.sh

# Копируем резервную копию
COPY Cursovaya.bak /backup/Cursovaya.bak

# Запускаем скрипт инициализации
CMD ["/bin/bash", "/init-database.sh"]

