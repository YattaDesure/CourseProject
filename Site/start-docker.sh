#!/usr/bin/env bash
# Один запуск: собрать API локально + поднять контейнеры (frontend собирается в Docker).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
"$ROOT/publish-backend.sh"
exec docker compose -f "$ROOT/docker-compose.yml" up -d --build
