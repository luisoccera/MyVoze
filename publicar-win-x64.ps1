$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    $destino = Join-Path $PSScriptRoot "Publicacion\win-x64"
    if (Test-Path -LiteralPath $destino) {
        Remove-Item -LiteralPath $destino -Recurse -Force
    }
    dotnet publish .\src\VoxMorph.Controlador\VoxMorph.Controlador.csproj `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $destino

    Write-Host "Publicacion creada en: $destino" -ForegroundColor Green
} finally {
    Pop-Location
}
