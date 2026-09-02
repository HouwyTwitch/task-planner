@echo off
REM Запускает сайт локально и открывает браузер.
REM
REM Локальный сервер нужен потому, что доступ к общей папке (File System
REM Access API) работает только по HTTPS или с localhost. Открытый двойным
REM кликом файл такого доступа не даёт.

setlocal
cd /d "%~dp0"

if not exist "site\index.html" (
  echo [ОШИБКА] Папка site\ пуста. Сначала соберите сайт: scripts\build.bat
  pause
  exit /b 1
)

if "%PORT%"=="" set PORT=4321

where node >nul 2>nul
if %ERRORLEVEL%==0 (
  start "" "http://localhost:%PORT%"
  node scripts\serve.mjs
  exit /b 0
)

where python >nul 2>nul
if %ERRORLEVEL%==0 (
  start "" "http://localhost:%PORT%"
  python -m http.server %PORT% --bind 127.0.0.1 --directory site
  exit /b 0
)

echo [ОШИБКА] Не найден ни Node.js, ни Python — нечем поднять локальный сервер.
echo Установите Node.js: https://nodejs.org
pause
exit /b 1
