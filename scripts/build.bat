@echo off
REM Собирает статический сайт: клонирует Super Productivity, вживляет наши
REM изменения и делает веб-сборку. Результат — в папке site\
REM Требуется Node.js 20+ и Git.

setlocal
cd /d "%~dp0.."

where node >nul 2>nul || (echo [ОШИБКА] Node.js не найден. Установите с https://nodejs.org & pause & exit /b 1)
where git  >nul 2>nul || (echo [ОШИБКА] Git не найден. Установите с https://git-scm.com & pause & exit /b 1)

if not exist "sp-src" (
  echo ==^> Клонирую Super Productivity...
  git clone --depth 1 https://github.com/johannesjo/super-productivity.git sp-src || (pause & exit /b 1)
)

echo ==^> Вживляю изменения ^(провайдер синхронизации + плагин^)...
node patch\apply.mjs || (pause & exit /b 1)

cd sp-src
echo ==^> Устанавливаю зависимости...
call npm install --no-audit --no-fund || (pause & exit /b 1)
echo ==^> Собираю внутренние пакеты...
call npm run build:packages || (pause & exit /b 1)
echo ==^> Собираю веб-версию...
call npm run buildFrontend:prodWeb || (pause & exit /b 1)
cd ..

echo ==^> Раскладываю результат в site\...
if exist "site" rmdir /s /q site
xcopy /e /i /q sp-src\dist\browser site >nul
del /s /q site\*.map >nul 2>nul
copy /y sp-src\dist\3rdpartylicenses.txt site\ >nul 2>nul
copy /y sp-src\LICENSE site\LICENSE.super-productivity >nul 2>nul

echo.
echo Готово. Статический сайт лежит в site\
echo Запустите start.bat, чтобы открыть его локально.
pause
