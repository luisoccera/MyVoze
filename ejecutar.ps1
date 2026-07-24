$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    $ejecutable = Join-Path $PSScriptRoot "Publicacion\win-x64\MyVoze.exe"
    if (-not (Test-Path -LiteralPath $ejecutable)) {
        throw "No se encontró MyVoze.exe. Ejecuta publicar-win-x64.ps1 desde el entorno de desarrollo."
    }

    & $ejecutable
} finally {
    Pop-Location
}
