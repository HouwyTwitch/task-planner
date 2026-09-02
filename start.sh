#!/usr/bin/env bash
# Запускает сайт локально и открывает браузер.
# Локальный сервер нужен потому, что доступ к общей папке (File System Access
# API) работает только по HTTPS или с localhost.
set -e
cd "$(dirname "$0")"

if [ ! -f site/index.html ]; then
  echo "[ОШИБКА] Папка site/ пуста. Сначала соберите сайт: scripts/build.sh"
  exit 1
fi

PORT="${PORT:-4321}"
if command -v node >/dev/null 2>&1; then
  exec node scripts/serve.mjs
elif command -v python3 >/dev/null 2>&1; then
  exec python3 -m http.server "$PORT" --bind 127.0.0.1 --directory site
else
  echo "[ОШИБКА] Нужен Node.js или Python."
  exit 1
fi
