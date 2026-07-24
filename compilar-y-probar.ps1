$ErrorActionPreference = "Stop"

Write-Host "Comprobando .NET 10..." -ForegroundColor Cyan
$version = & dotnet --version
if (-not $version.StartsWith("10.")) {
    throw "VoxMorph necesita .NET SDK 10. Instala el SDK y vuelve a ejecutar este archivo. Version encontrada: $version"
}
Push-Location $PSScriptRoot
try {
    dotnet restore .\VoxMorph.sln
    dotnet build .\VoxMorph.sln -c Release --no-restore
    dotnet test .\VoxMorph.sln -c Release --no-build
    Write-Host "VoxMorph compilo y todas las pruebas terminaron correctamente." -ForegroundColor Green
} finally {
    Pop-Location
}
