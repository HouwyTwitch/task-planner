@echo off
REM Автозапуск при входе в систему (без прав администратора).
REM Кладёт ярлык в папку "Автозагрузка" пользователя.

setlocal
cd /d "%~dp0"

set "TARGET=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\TaskPlanner.lnk"
set "SCRIPT=%~dp0run.bat"

powershell -NoProfile -Command ^
  "$s=(New-Object -ComObject WScript.Shell).CreateShortcut('%TARGET%');" ^
  "$s.TargetPath='%SCRIPT%'; $s.WorkingDirectory='%~dp0'; $s.WindowStyle=7; $s.Save()"

echo Готово. Ярлык создан в автозагрузке: %TARGET%
echo При следующем входе в Windows приложение будет запускаться автоматически.
pause
