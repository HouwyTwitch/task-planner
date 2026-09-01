@echo off
setlocal
cd /d "%~dp0"

where py >nul 2>nul
if %ERRORLEVEL%==0 (
  set "PY=py -3"
) else (
  where python >nul 2>nul
  if %ERRORLEVEL%==0 (
    set "PY=python"
  ) else (
    echo [ERROR] Python 3.11+ не найден. Установите его с https://www.python.org/downloads/ и повторите.
    pause
    exit /b 1
  )
)

echo [1/4] Создаю виртуальное окружение...
if not exist ".venv" (
  %PY% -m venv .venv || (echo [ERROR] Не удалось создать venv & pause & exit /b 1)
)

echo [2/4] Обновляю pip...
call ".venv\Scripts\python.exe" -m pip install --upgrade pip wheel setuptools

echo [3/4] Устанавливаю зависимости...
call ".venv\Scripts\python.exe" -m pip install -r requirements.txt || (echo [ERROR] Ошибка установки зависимостей & pause & exit /b 1)

echo [4/4] Генерирую self-signed HTTPS-сертификат...
call ".venv\Scripts\python.exe" scripts\make_cert.py

echo.
echo ============================================================
echo Готово. Запустите run.bat.
echo Первый зарегистрированный пользователь станет администратором.
echo ============================================================
pause
