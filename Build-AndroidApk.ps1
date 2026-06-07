# Builds AR Education as an Android APK and saves it to the Desktop.
# Usage:
#   powershell -ExecutionPolicy Bypass -File .\Build-AndroidApk.ps1
# Optional:
#   .\Build-AndroidApk.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.4.8f1\Editor\Unity.exe"

param(
    [string]$UnityPath = "",
    [string]$ProjectPath = "",
    [string]$ApkName = "AR-Education.apk"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n== $Message ==" -ForegroundColor Cyan
}

function Resolve-ProjectPath {
    if (-not [string]::IsNullOrWhiteSpace($ProjectPath)) {
        return (Resolve-Path $ProjectPath).Path
    }

    return $PSScriptRoot
}

function Find-Unity {
    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) {
        if (Test-Path $UnityPath) { return (Resolve-Path $UnityPath).Path }
        throw "UnityPath was provided but does not exist: $UnityPath"
    }

    $candidates = @()

    if ($IsWindows -or $env:OS -eq "Windows_NT") {
        $roots = @(
            "$env:PROGRAMFILES\Unity\Hub\Editor",
            "${env:PROGRAMFILES(X86)}\Unity\Hub\Editor",
            "C:\Unity\Hub\Editor"
        )

        foreach ($root in $roots) {
            if (Test-Path $root) {
                $candidates += Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue |
                    Sort-Object Name -Descending |
                    ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" }
            }
        }
    } else {
        $candidates += "/Applications/Unity/Hub/Editor/6000.4.8f1/Unity.app/Contents/MacOS/Unity"
        $candidates += "/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity"

        if (Test-Path "/Applications/Unity/Hub/Editor") {
            $candidates += Get-ChildItem -Path "/Applications/Unity/Hub/Editor" -Directory -ErrorAction SilentlyContinue |
                Sort-Object Name -Descending |
                ForEach-Object { Join-Path $_.FullName "Unity.app/Contents/MacOS/Unity" }
        }
    }

    $unity = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if ($unity) { return $unity }

    throw "Unity Editor was not found. Install Unity with Android Build Support or pass -UnityPath."
}

$ResolvedProjectPath = Resolve-ProjectPath
$Unity = Find-Unity
$Desktop = [Environment]::GetFolderPath("DesktopDirectory")
if ([string]::IsNullOrWhiteSpace($Desktop)) {
    $Desktop = Join-Path $HOME "Desktop"
}

$OutputDir = Join-Path $Desktop "AR-Education-Builds"
$ApkPath = Join-Path $OutputDir $ApkName
$LogPath = Join-Path $OutputDir "AR-Education-Android-Build.log"

Write-Step "Preparing Android APK build"
Write-Host "Project: $ResolvedProjectPath"
Write-Host "Unity:   $Unity"
Write-Host "APK:     $ApkPath"
Write-Host "Log:     $LogPath"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Step "Running Unity batch build"
& $Unity `
    -batchmode `
    -quit `
    -projectPath "$ResolvedProjectPath" `
    -buildTarget Android `
    -executeMethod AREducation.Editor.AndroidApkBuilder.BuildFromCommandLine `
    -apkPath "$ApkPath" `
    -logFile "$LogPath"

$ExitCode = $LASTEXITCODE
if ($ExitCode -ne 0) {
    Write-Host "`nBuild failed with exit code $ExitCode." -ForegroundColor Red
    Write-Host "Open the log for details: $LogPath" -ForegroundColor Yellow
    exit $ExitCode
}

if (-not (Test-Path $ApkPath)) {
    Write-Host "`nUnity exited successfully, but the APK was not found at: $ApkPath" -ForegroundColor Red
    Write-Host "Open the log for details: $LogPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nAPK created successfully:" -ForegroundColor Green
Write-Host $ApkPath -ForegroundColor Green
