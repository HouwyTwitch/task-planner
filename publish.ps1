param(
    [string]$Runtime = "win-x64",
    # Сетевая папка обновлений: если указана, готовый архив и update.json копируются в неё.
    [string]$UpdateFolder = "",
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$Platform = switch -Regex ($Runtime) {
    'win-x64'   { 'x64'; break }
    'win-x86'   { 'x86'; break }
    'win-arm64' { 'arm64'; break }
    default { Write-Error "Поддерживаются Runtime: win-x64, win-x86, win-arm64"; exit 4 }
}

dotnet publish .\src\Planner.App\Planner.App.csproj -c Release -r $Runtime --self-contained true -p:WindowsAppSDKSelfContained=true -p:Platform=$Platform -p:PublishSingleFile=false -o .\publish\$Runtime
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish завершился с ошибкой, код $LASTEXITCODE"
    exit $LASTEXITCODE
}

$publishDir = Join-Path $root "publish\$Runtime"
$configPath = Join-Path $publishDir "planner.settings.json"
$exePath = Join-Path $publishDir "Сетевой планировщик.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "Публикация завершена, но файл «Сетевой планировщик.exe» не найден в $publishDir"
    exit 3
}

if (-not (Test-Path $configPath)) {
    Write-Error "Публикация завершена, но planner.settings.json не скопирован в $publishDir"
    exit 2
}

Write-Host "Публикация готова: publish\$Runtime" -ForegroundColor Green
Write-Host "Путь к сетевой папке настраивается здесь: $configPath" -ForegroundColor Cyan
Write-Host "Исполняемый файл: $exePath" -ForegroundColor Cyan

# --- Пакет обновления ---------------------------------------------------------
# Программа обновляется из ZIP-архива в сетевой папке. Рядом с архивом лежит
# небольшой update.json: по нему проверка новой версии занимает доли секунды.

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath).FileVersion
$version = ($version -split '\.')[0..2] -join '.'

$archiveName = "Планировщик-$version-$Runtime.zip"
$archivePath = Join-Path $root "publish\$archiveName"
if (Test-Path $archivePath) { Remove-Item $archivePath -Force }
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $archivePath -CompressionLevel Optimal

$manifest = [ordered]@{
    Version    = $version
    File       = $archiveName
    ReleasedAt = (Get-Date).ToString('s')
    Notes      = $Notes
}
$manifestPath = Join-Path $root "publish\update.json"
$manifest | ConvertTo-Json | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host "Архив обновления: $archivePath" -ForegroundColor Green
Write-Host "Описание версии: $manifestPath" -ForegroundColor Green

if ($UpdateFolder) {
    if (-not (Test-Path $UpdateFolder)) { New-Item -ItemType Directory -Path $UpdateFolder -Force | Out-Null }
    Copy-Item $archivePath  -Destination $UpdateFolder -Force
    # update.json копируется последним: до этого момента клиенты видят прежнюю версию.
    Copy-Item $manifestPath -Destination $UpdateFolder -Force
    Write-Host "Обновление выложено в $UpdateFolder" -ForegroundColor Green
}
