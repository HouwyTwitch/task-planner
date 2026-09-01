#!/usr/bin/env bash
# Пересобирает Super Productivity из sp-src/ и кладёт результат в sp-dist/.
# Нужен только когда вы обновили sp-src (например, подтянули новую версию).
# Требует Node.js 20+.
set -e
cd "$(dirname "$0")/.."

if [ ! -d sp-src ]; then
  echo "sp-src/ отсутствует. Клонирую super-productivity..."
  git clone --depth 1 https://github.com/johannesjo/super-productivity.git sp-src
fi

cd sp-src
npm install --no-audit --no-fund
npm run build:packages
npm run buildFrontend:prodWeb
cd ..

rm -rf sp-dist
cp -r sp-src/dist/browser sp-dist
# Убираем .map-файлы для меньшего веса
find sp-dist -name "*.map" -delete
# GPL: сохраняем список лицензий и текст лицензии SP
cp sp-src/dist/3rdpartylicenses.txt sp-dist/ 2>/dev/null || true
cp sp-src/LICENSE sp-dist/LICENSE.super-productivity 2>/dev/null || true

echo "Готово. sp-dist/ обновлён."
