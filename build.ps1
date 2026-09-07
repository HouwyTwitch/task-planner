$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

dotnet restore .\Planner.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet restore завершился с ошибкой, код $LASTEXITCODE"
    exit $LASTEXITCODE
}

dotnet build .\Planner.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet build завершился с ошибкой, код $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Сборка успешно завершена" -ForegroundColor Green
