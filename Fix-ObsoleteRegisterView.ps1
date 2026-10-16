$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot
$projectFile = Join-Path $repoRoot 'Backend\PenielPlaceMinistries.csproj'
$obsoleteView = Join-Path $repoRoot 'Backend\Views\Auth\Register.cshtml'

if (-not (Test-Path $projectFile)) {
    throw "Could not find Backend\PenielPlaceMinistries.csproj. Save this script in the repository root (the folder containing Backend)."
}

if (Test-Path $obsoleteView) {
    Remove-Item -LiteralPath $obsoleteView -Force
    Write-Host 'Removed obsolete Backend\Views\Auth\Register.cshtml.' -ForegroundColor Green
} else {
    Write-Host 'Register.cshtml was not present at the expected path; no view was removed.' -ForegroundColor Yellow
}

# Remove generated Razor/compiler output so the next build regenerates it from current source.
foreach ($relativePath in @('Backend\bin', 'Backend\obj')) {
    $path = Join-Path $repoRoot $relativePath
    if (Test-Path $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
        Write-Host "Removed generated build folder: $relativePath"
    }
}

# Show any remaining source reference that may need separate cleanup.
$backendRoot = Join-Path $repoRoot 'Backend'
$remaining = Get-ChildItem -LiteralPath $backendRoot -Recurse -File -Include '*.cs', '*.cshtml' |
    Where-Object { $_.FullName -notmatch '[\\/]((bin)|(obj))[\\/]' } |
    Select-String -SimpleMatch 'RegisterRequest' -ErrorAction SilentlyContinue

if ($remaining) {
    Write-Host 'Warning: additional source files still reference RegisterRequest:' -ForegroundColor Yellow
    $remaining | ForEach-Object { Write-Host ("{0}:{1}: {2}" -f $_.Path, $_.LineNumber, $_.Line.Trim()) }
} else {
    Write-Host 'No remaining RegisterRequest references were found in Backend source.' -ForegroundColor Green
}

Write-Host ''
Write-Host 'Next: run dotnet build .\Backend\PenielPlaceMinistries.csproj -c Release' -ForegroundColor Cyan
