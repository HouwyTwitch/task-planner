#!/usr/bin/env bash
# Собирает статический сайт: клонирует Super Productivity, вживляет наши
# изменения и делает веб-сборку. Результат — в папке site/.
#
# Требуется Node.js 20+.
set -e
cd "$(dirname "$0")/.."

if [ ! -d sp-src ]; then
  echo "==> Клонирую Super Productivity..."
  git clone --depth 1 https://github.com/johannesjo/super-productivity.git sp-src
fi

echo "==> Вживляю изменения (провайдер синхронизации + плагин)..."
node patch/apply.mjs

echo "==> Устанавливаю зависимости..."
cd sp-src
npm install --no-audit --no-fund

echo "==> Собираю внутренние пакеты..."
npm run build:packages

echo "==> Собираю веб-версию..."
npm run buildFrontend:prodWeb
cd ..

echo "==> Раскладываю результат в site/..."
rm -rf site
cp -r sp-src/dist/browser site
find site -name "*.map" -delete

# Плагин Google Calendar несёт в себе OAuth client id разработчиков Super
# Productivity. Это не утечка, но защита от секретов в GitHub блокирует
# коммит такой сборки, а в локальной сети без интернета плагин всё равно
# бесполезен. Если интернет есть и календарь нужен — уберите эту строку.
rm -rf site/assets/bundled-plugins/google-calendar-provider

# Лицензии Super Productivity (GPLv3) обязаны ехать вместе со сборкой
cp sp-src/dist/3rdpartylicenses.txt site/ 2>/dev/null || true
cp sp-src/LICENSE site/LICENSE.super-productivity 2>/dev/null || true

echo ""
echo "Готово. Статический сайт лежит в site/"
echo "Разложите его на любой веб-сервер и откройте по HTTPS или с localhost."
