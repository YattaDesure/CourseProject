#!/usr/bin/env bash
# Применить все SQL из init-db/upgrade/ к БД Cursovaya в контейнере greenquarter-sqlserver.
# Запуск из корня проекта:  bash init-db/apply-upgrades-docker.sh
set -euo pipefail
SA_PASSWORD="${SA_PASSWORD:-22332123Yaz}"
CONTAINER="${SQLSERVER_CONTAINER:-greenquarter-sqlserver}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
UPGRADE_DIR="$SCRIPT_DIR/upgrade"

if ! docker ps --format '{{.Names}}' | grep -qx "$CONTAINER"; then
  echo "Контейнер $CONTAINER не запущен. Запустите: docker compose up -d sqlserver"
  exit 1
fi

shopt -s nullglob
files=( "$UPGRADE_DIR"/*.sql )
if [ ${#files[@]} -eq 0 ]; then
  echo "Нет файлов в $UPGRADE_DIR"
  exit 0
fi

IFS=$'\n' sorted=( $(printf '%s\n' "${files[@]}" | sort) )
unset IFS

for f in "${sorted[@]}"; do
  echo "==> $f"
  docker exec -i "$CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U SA -P "$SA_PASSWORD" -d Cursovaya -C -i "/scripts/upgrade/$(basename "$f")"
done

echo "Готово."
