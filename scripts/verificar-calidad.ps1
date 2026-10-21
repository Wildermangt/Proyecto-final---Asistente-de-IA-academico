<#
.SYNOPSIS
    Repite las verificaciones automaticas de calidad y seguridad del plan de pruebas
    (Q01, Q02, U01-U04 y S01-S05 de docs/05-plan-pruebas.md).

.DESCRIPTION
    Ejecutar desde la raiz del repositorio, en Windows, con el SDK de .NET 10 y la carga
    de trabajo de .NET MAUI instalados:

        powershell -ExecutionPolicy Bypass -File scripts/verificar-calidad.ps1

    Use -OmitirAndroid para saltar Q02 si el equipo no tiene el SDK de Android.
    Q03 y Q04 (APK y ejecutable Release), S06 y las pruebas de interfaz (P01-P10, A12, F17)
    se siguen ejecutando de forma manual con capturas.

    El script termina con codigo 0 si todos los casos se aprueban y 1 si alguno falla.
#>
param(
    [switch]$OmitirAndroid
)

$ErrorActionPreference = 'Continue'
$proyecto = 'AsistenteIA/AsistenteIA.csproj'
$resultados = New-Object System.Collections.Generic.List[object]

function Registrar([string]$caso, [bool]$aprobado, [string]$detalle) {
    $estado = if ($aprobado) { 'Aprobado' } else { 'FALLA' }
    $resultados.Add([pscustomobject]@{ Caso = $caso; Estado = $estado; Detalle = $detalle })
    $color = if ($aprobado) { 'Green' } else { 'Red' }
    Write-Host ("[{0}] {1}: {2}" -f $estado, $caso, $detalle) -ForegroundColor $color
}

if (-not (Test-Path $proyecto)) {
    Write-Host "No se encontro $proyecto. Ejecute el script desde la raiz del repositorio." -ForegroundColor Red
    exit 1
}

# Q01: compilacion para Windows sin errores ni advertencias.
# -warnaserror convierte cualquier advertencia en error; --no-incremental evita que una
# compilacion previa oculte advertencias.
Write-Host "`nQ01: compilando para Windows..."
dotnet build $proyecto -f net10.0-windows10.0.19041.0 --no-incremental -warnaserror | Out-Host
Registrar 'Q01' ($LASTEXITCODE -eq 0) 'dotnet build Windows: 0 errores y 0 advertencias'

# Q02: compilacion para Android sin errores (las advertencias XA del SQLite nativo se aceptan).
if ($OmitirAndroid) {
    Write-Host "`nQ02: omitido (-OmitirAndroid)."
} else {
    Write-Host "`nQ02: compilando para Android..."
    dotnet build $proyecto -f net10.0-android | Out-Host
    Registrar 'Q02' ($LASTEXITCODE -eq 0) 'dotnet build Android: 0 errores'
}

# U01-U04: pruebas unitarias xUnit del proyecto AsistenteIA.Tests.
Write-Host "`nU01-U04: ejecutando pruebas unitarias..."
dotnet test AsistenteIA.Tests | Out-Host
Registrar 'U01-U04' ($LASTEXITCODE -eq 0) 'dotnet test: todas las pruebas unitarias aprobadas'

# S01 y S02: paquetes vulnerables u obsoletos. dotnet list package marca cada paquete
# encontrado con una fila que empieza por ">", sin importar el idioma del SDK.
Write-Host "`nS01: buscando paquetes vulnerables..."
$vulnerables = dotnet list $proyecto package --vulnerable --include-transitive 2>&1 | Out-String
$filasVulnerables = @($vulnerables -split "`r?`n" | Where-Object { $_ -match '^\s*>\s' })
Registrar 'S01' ($filasVulnerables.Count -eq 0) ("paquetes vulnerables: {0}" -f $filasVulnerables.Count)

Write-Host "`nS02: buscando paquetes obsoletos..."
$obsoletos = dotnet list $proyecto package --deprecated 2>&1 | Out-String
$filasObsoletas = @($obsoletos -split "`r?`n" | Where-Object { $_ -match '^\s*>\s' })
Registrar 'S02' ($filasObsoletas.Count -eq 0) ("paquetes obsoletos: {0}" -f $filasObsoletas.Count)

# S03: claves reales en los archivos versionados. Solo coincide con valores largos, de modo
# que la palabra "Bearer" del codigo y la clave falsa de P04 no cuentan como hallazgo.
Write-Host "`nS03: buscando claves en el repositorio..."
$patronClaves = 'sk_[A-Za-z0-9]{20,}|pk_[A-Za-z0-9]{20,}|AIza[0-9A-Za-z_-]{35}|Bearer [A-Za-z0-9._-]{20,}'
$claves = @(git grep -n -I -E $patronClaves)
$claves | Out-Host
Registrar 'S03' ($claves.Count -eq 0) ("posibles claves encontradas: {0}" -f $claves.Count)

# S04: bin/ y obj/ fuera de Git.
Write-Host "`nS04: revisando bin/ y obj/..."
$compilados = @(git ls-files | Where-Object { $_ -match '(^|/)(bin|obj)/' })
Registrar 'S04' ($compilados.Count -eq 0) ("archivos de bin/ u obj/ versionados: {0}" -f $compilados.Count)

# S05: el codigo solo usa URL HTTPS (se revisan las cadenas "http://" en los .cs).
Write-Host "`nS05: buscando URL sin HTTPS..."
$urlsHttp = @(git grep -n -I -F '"http://' -- 'AsistenteIA/*.cs')
$urlsHttp | Out-Host
Registrar 'S05' ($urlsHttp.Count -eq 0) ("URL http:// en el codigo: {0}" -f $urlsHttp.Count)

Write-Host "`nResumen"
$resultados | Format-Table -AutoSize | Out-Host

$fallas = @($resultados | Where-Object { $_.Estado -ne 'Aprobado' }).Count
if ($fallas -gt 0) {
    Write-Host "$fallas caso(s) con falla." -ForegroundColor Red
    exit 1
}
Write-Host 'Todos los casos aprobados.' -ForegroundColor Green
exit 0
