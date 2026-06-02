# =============================================================================
# AR Education MVP — Setup Script (Windows PowerShell)
# Clones the repo, checks out the project branch, and guides Unity setup.
# Usage: .\setup.ps1 [-TargetDir "MyFolder"]
# Run as: PowerShell -ExecutionPolicy Bypass -File setup.ps1
# =============================================================================

param(
    [string]$TargetDir = "AR-Education-MVP"
)

$RepoUrl     = "https://github.com/r00texploit/AR-project.git"
$Branch      = "claude/ar-education-mvp-CWdbk"
$UnityVersion = "2022.3"

# ── Helpers ───────────────────────────────────────────────────────────────────
function Write-Step  { param($msg) Write-Host "`n── $msg ──" -ForegroundColor Cyan }
function Write-OK    { param($msg) Write-Host "[OK] $msg"   -ForegroundColor Green }
function Write-Warn  { param($msg) Write-Host "[!]  $msg"   -ForegroundColor Yellow }
function Write-Fail  { param($msg) Write-Host "[X]  $msg"   -ForegroundColor Red }
function Write-Info  { param($msg) Write-Host "     $msg" }

# ── Banner ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  AR Education MVP — Setup" -ForegroundColor Cyan -BackgroundColor DarkBlue
Write-Host ""
Write-Info "Repository : $RepoUrl"
Write-Info "Branch     : $Branch"
Write-Info "Directory  : $TargetDir"
Write-Info "Unity ver  : $UnityVersion LTS (required)"
Write-Host ""

# ── 1. Prerequisites ──────────────────────────────────────────────────────────
Write-Step "Checking prerequisites"

# Git
$GitOk = $false
try {
    $gitVer = & git --version 2>&1
    Write-OK "Git found: $gitVer"
    $GitOk = $true
} catch {
    Write-Fail "Git not found. Download: https://git-scm.com/downloads"
}

if (-not $GitOk) {
    Write-Fail "Missing required tools. Install Git and re-run."
    exit 1
}

# Unity Hub
$UnityHubPaths = @(
    "$env:PROGRAMFILES\Unity Hub\Unity Hub.exe",
    "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe"
)
$UnityHubExe = $UnityHubPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($UnityHubExe) {
    Write-OK "Unity Hub found: $UnityHubExe"
} else {
    Write-Warn "Unity Hub not found. Download: https://unity.com/download"
}

# Unity Editor
$UnityEditorPath = $null
$UnitySearchRoots = @(
    "$env:PROGRAMFILES\Unity\Hub\Editor",
    "C:\Unity\Hub\Editor"
)
foreach ($root in $UnitySearchRoots) {
    if (Test-Path $root) {
        $match = Get-ChildItem -Path $root -Directory |
                 Where-Object { $_.Name -like "$UnityVersion*" } |
                 Sort-Object Name | Select-Object -Last 1
        if ($match) { $UnityEditorPath = $match.FullName; break }
    }
}

if ($UnityEditorPath) {
    Write-OK "Unity $UnityVersion found: $UnityEditorPath"
} else {
    Write-Warn "Unity $UnityVersion LTS not detected."
    Write-Info "Install via Unity Hub → Installs → Install Editor → $UnityVersion LTS"
}

# ── 2. Clone / update ─────────────────────────────────────────────────────────
Write-Step "Cloning repository"

if (Test-Path (Join-Path $TargetDir ".git")) {
    Write-Warn "'$TargetDir' is already a git repo — fetching latest..."
    git -C $TargetDir fetch origin $Branch
    git -C $TargetDir checkout $Branch
    git -C $TargetDir pull origin $Branch
    Write-OK "Repository updated."
} elseif (Test-Path $TargetDir) {
    $items = Get-ChildItem $TargetDir -ErrorAction SilentlyContinue
    if ($items) {
        Write-Fail "'$TargetDir' exists and is not empty. Use a different name:"
        Write-Info "  .\setup.ps1 -TargetDir MyNewFolder"
        exit 1
    }
    git clone --branch $Branch --single-branch --depth 1 $RepoUrl $TargetDir
    Write-OK "Repository cloned."
} else {
    git clone --branch $Branch --single-branch --depth 1 $RepoUrl $TargetDir
    Write-OK "Repository cloned into '$TargetDir'."
}

$FullPath = (Resolve-Path $TargetDir).Path

# ── 3. Verify structure ────────────────────────────────────────────────────────
Write-Step "Verifying project structure"

$RequiredPaths = @(
    "Assets\Scripts",
    "Assets\Scenes",
    "Assets\Resources\QuizData",
    "Packages\manifest.json",
    "ProjectSettings\ProjectSettings.asset"
)

$AllOk = $true
foreach ($p in $RequiredPaths) {
    $full = Join-Path $FullPath $p
    if (Test-Path $full) {
        Write-OK $p
    } else {
        Write-Fail "Missing: $p"
        $AllOk = $false
    }
}

if (-not $AllOk) {
    Write-Fail "Project structure incomplete. Clone may have failed."
    exit 1
}

$ScriptCount = (Get-ChildItem -Path (Join-Path $FullPath "Assets\Scripts") -Filter "*.cs" -Recurse).Count
Write-OK "C# scripts found: $ScriptCount"

# ── 4. Open in Unity Hub ──────────────────────────────────────────────────────
Write-Step "Unity Hub integration"

if ($UnityHubExe) {
    $answer = Read-Host "`n  Open the project in Unity Hub now? [Y/n]"
    if ($answer -eq "" -or $answer -match "^[Yy]") {
        Start-Process $UnityHubExe -ArgumentList "-- --headless open `"$FullPath`"" -ErrorAction SilentlyContinue
        Write-OK "Project sent to Unity Hub."
    }
}

# ── 5. Next steps ─────────────────────────────────────────────────────────────
Write-Step "Next Steps"

$n = 1
Write-Host ""
if (-not $UnityHubExe) {
    Write-Host "  $($n++). Open in Unity Hub" -ForegroundColor White
    Write-Info "     Unity Hub → Projects → Add → select:"
    Write-Host "     $FullPath" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "  $($n++). Set Unity version" -ForegroundColor White
Write-Info "     Select Unity $UnityVersion.x LTS when prompted"
Write-Host ""

Write-Host "  $($n++). Wait for package resolution (~2-5 min)" -ForegroundColor White
Write-Info "     AR Foundation 5.1.2, ARCore/ARKit, TextMeshPro auto-download"
Write-Host ""

Write-Host "  $($n++). Build scene hierarchy" -ForegroundColor White
Write-Host "     Menu bar → AR Education > Setup All Scenes" -ForegroundColor Yellow
Write-Host ""

Write-Host "  $($n++). Enable AR plugins" -ForegroundColor White
Write-Info "     Edit → Project Settings → XR Plug-in Management"
Write-Host "     Android tab → check ARCore" -ForegroundColor Yellow
Write-Host ""

Write-Host "  $($n++). Test in Editor (no device needed)" -ForegroundColor White
Write-Info "     Open Assets/Scenes/Quiz.unity → Press Play"
Write-Info "     Open Assets/Scenes/TeacherDashboard.unity → Press Play"
Write-Host ""

Write-Host "  $($n++). Build for Android" -ForegroundColor White
Write-Info "     File → Build Settings → Android → Switch Platform"
Write-Info "     Bundle ID: com.areducation.mvp | Min SDK: 24 | IL2CPP | ARM64"
Write-Info "     Build and Run (device must support ARCore)"
Write-Host ""

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step "Setup Complete"
Write-Host ""
Write-OK "Project ready at: $FullPath"
Write-Host ""
Write-Info "Scenes  : MainMenu | ARLesson | Quiz | TeacherDashboard"
Write-Info "Scripts : $ScriptCount C# files"
Write-Info "Quizzes : triangle | physics | cube (5 questions each)"
Write-Host ""
