@echo off
setlocal
cd /d "%~dp0"

if not exist ".venv\Scripts\python.exe" (
  echo [ERROR] Виртуальное окружение не найдено. Сначала запустите install.bat
  pause
  exit /b 1
)

if not exist "data\cert.pem" (
  echo [WARN] Нет data\cert.pem — генерирую...
  call ".venv\Scripts\python.exe" scripts\make_cert.py
)

set HOST=0.0.0.0
set PORT=8443

echo Запуск: https://<адрес-этой-машины>:%PORT%
echo (все машины в локальной сети смогут подключиться после установки cert.pem)
echo.

call ".venv\Scripts\python.exe" -m uvicorn backend.main:app ^
  --host %HOST% --port %PORT% ^
  --ssl-keyfile data\key.pem --ssl-certfile data\cert.pem
