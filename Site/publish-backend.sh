#!/usr/bin/env bash
# Собирает API в backend/publish для Docker (без образа sdk — обход проблем с mcr.microsoft.com).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
echo "→ dotnet publish → $ROOT/backend/publish"
dotnet publish "$ROOT/backend/GreenQuarter.Api/GreenQuarter.Api.csproj" -c Release -o "$ROOT/backend/publish"
echo "✓ Готово. Дальше: docker compose up -d"
