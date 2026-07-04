<#
.SYNOPSIS
    Publishes EduLSM as a self-contained, single-file build for Windows, Linux and macOS.

.DESCRIPTION
    Produces one folder per platform under ./publish/<rid>/ containing a single
    executable (the Roboto font is bundled inside it), plus a distributable .zip
    per platform.

.EXAMPLE
    ./publish-all.ps1
    ./publish-all.ps1 -Rids win-x64,linux-x64
    ./publish-all.ps1 -Dotnet "$env:USERPROFILE\.dotnet\dotnet.exe"
#>
param(
    # Target runtimes. Add win-arm64 / linux-arm64 here if you need them.
    [string[]] $Rids = @('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64'),

    # Override if "dotnet" on PATH reports "No .NET SDKs were found"
    # (e.g. "$env:USERPROFILE\.dotnet\dotnet.exe").
    [string]   $Dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
$Project = './src/Main'
$OutRoot = './publish'

$Flags = @(
    '-c', 'Release'
    '--self-contained'
    '/p:PublishSingleFile=true'
    '/p:IncludeAllContentForSelfExtract=true'
    '/p:IncludeNativeLibsForSelfExtract=true'
    '/p:PublishReadyToRun=true'
    '/p:PublishTrimmed=true'
    '/p:DeleteExistingFiles=true'
    '/p:DebugType=none'
    '/p:DebugSymbols=false'
)

foreach ($rid in $Rids) {
    $outDir = Join-Path $OutRoot $rid
    Write-Host "==> Publishing $rid ..." -ForegroundColor Cyan

    & $Dotnet publish $Project -r $rid -o $outDir @Flags --nologo
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $rid (exit $LASTEXITCODE)" }

    # Zip for distribution. Retry briefly: on Windows, antivirus/Defender often
    # holds a short-lived lock on the just-written .exe. NOTE: zips created on
    # Windows do not carry the Unix execute bit, so Linux/macOS users must run
    # `chmod +x EduLSM` after extracting.
    $zip = Join-Path $OutRoot "EduLSM-$rid.zip"
    if (Test-Path $zip) { Remove-Item $zip }
    for ($attempt = 1; ; $attempt++) {
        try {
            Compress-Archive -Path (Join-Path $outDir '*') -DestinationPath $zip -Force -ErrorAction Stop
            break
        }
        catch {
            if ($attempt -ge 5) { throw "Failed to zip $rid after $attempt attempts: $($_.Exception.Message)" }
            Start-Sleep -Seconds 2
        }
    }

    Write-Host "    done: $outDir  ->  $zip" -ForegroundColor Green
}

Write-Host "`nAll builds complete in $OutRoot" -ForegroundColor Green
