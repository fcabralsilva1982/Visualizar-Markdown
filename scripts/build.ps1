$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root "src\MarkdownViewer.cs"
$assemblyInfo = Join-Path $root "src\Properties\AssemblyInfo.cs"
$icon = Join-Path $root "assets\app.ico"
$outputDir = Join-Path $root "dist"
$exe = Join-Path $outputDir "MarkdownViewer.exe"
$compiler = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (!(Test-Path $compiler)) {
    $compiler = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

if (!(Test-Path $compiler)) {
    throw "Compilador C# do .NET Framework nao encontrado."
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$iconArg = if (Test-Path $icon) { "/win32icon:$icon" } else { "" }

& $compiler /nologo /target:winexe /platform:anycpu /out:$exe $iconArg /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $source $assemblyInfo
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao compilar MarkdownViewer.exe. Feche o executavel se ele estiver aberto e tente novamente."
}

Write-Host "Gerado: $exe"
