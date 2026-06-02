#!/usr/bin/env bash
# =============================================================================
# AR Education MVP — Setup Script
# Clones the repo, checks out the project branch, and guides Unity setup.
# Usage: bash setup.sh [target-directory]
# =============================================================================

set -euo pipefail

REPO_URL="https://github.com/r00texploit/AR-project.git"
BRANCH="claude/ar-education-mvp-CWdbk"
DEFAULT_DIR="AR-Education-MVP"
UNITY_VERSION="2022.3"

# ── Colors ────────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

log()     { echo -e "${BOLD}[setup]${RESET} $*"; }
success() { echo -e "${GREEN}${BOLD}[✓]${RESET} $*"; }
warn()    { echo -e "${YELLOW}${BOLD}[!]${RESET} $*"; }
error()   { echo -e "${RED}${BOLD}[✗]${RESET} $*" >&2; }
step()    { echo -e "\n${CYAN}${BOLD}──────────────────────────────────────${RESET}"; \
            echo -e "${CYAN}${BOLD} $*${RESET}"; \
            echo -e "${CYAN}${BOLD}──────────────────────────────────────${RESET}"; }

# ── Target directory ──────────────────────────────────────────────────────────
TARGET_DIR="${1:-$DEFAULT_DIR}"

step "AR Education MVP — Project Setup"
echo ""
log "Repository : $REPO_URL"
log "Branch     : $BRANCH"
log "Directory  : $TARGET_DIR"
log "Unity ver  : $UNITY_VERSION LTS (required)"
echo ""

# ── 1. Prerequisite checks ────────────────────────────────────────────────────
step "Checking prerequisites"

check_command() {
    local cmd="$1" label="${2:-$1}" install_hint="${3:-}"
    if command -v "$cmd" &>/dev/null; then
        success "$label found: $(command -v "$cmd")"
        return 0
    else
        error "$label not found."
        [[ -n "$install_hint" ]] && warn "  Install: $install_hint"
        return 1
    fi
}

PREREQS_OK=true

check_command git "Git" \
    "https://git-scm.com/downloads" || PREREQS_OK=false

# Unity Hub detection (optional — just warn)
UNITY_HUB_FOUND=false
if command -v unityhub &>/dev/null; then
    UNITY_HUB_FOUND=true
    success "Unity Hub found"
elif [[ -d "/Applications/Unity Hub.app" ]]; then
    UNITY_HUB_FOUND=true
    success "Unity Hub found at /Applications/Unity Hub.app"
elif [[ -f "$HOME/Applications/Unity Hub.AppImage" ]]; then
    UNITY_HUB_FOUND=true
    success "Unity Hub found at ~/Applications/Unity Hub.AppImage"
else
    warn "Unity Hub not detected. Download: https://unity.com/download"
fi

# Unity Editor detection
UNITY_FOUND=false
UNITY_PATH=""

# macOS paths
for p in \
    "/Applications/Unity/Hub/Editor" \
    "$HOME/Applications/Unity/Hub/Editor"; do
    if [[ -d "$p" ]]; then
        # Find the 2022.3.x version
        match=$(find "$p" -maxdepth 1 -name "${UNITY_VERSION}*" -type d 2>/dev/null | sort -V | tail -1)
        if [[ -n "$match" ]]; then
            UNITY_FOUND=true
            UNITY_PATH="$match"
            break
        fi
    fi
done

# Linux paths
if [[ "$UNITY_FOUND" == false ]]; then
    for p in \
        "$HOME/Unity/Hub/Editor" \
        "/opt/unity/editor" \
        "/opt/Unity"; do
        if [[ -d "$p" ]]; then
            match=$(find "$p" -maxdepth 2 -name "${UNITY_VERSION}*" -type d 2>/dev/null | sort -V | tail -1)
            if [[ -n "$match" ]]; then
                UNITY_FOUND=true
                UNITY_PATH="$match"
                break
            fi
        fi
    done
fi

if [[ "$UNITY_FOUND" == true ]]; then
    success "Unity $UNITY_VERSION found: $UNITY_PATH"
else
    warn "Unity $UNITY_VERSION LTS not detected."
    warn "  Download via Unity Hub → Installs → Install Editor → $UNITY_VERSION LTS"
fi

if [[ "$PREREQS_OK" == false ]]; then
    error "Missing required tools. Please install them and re-run this script."
    exit 1
fi

# ── 2. Clone repository ───────────────────────────────────────────────────────
step "Cloning repository"

if [[ -d "$TARGET_DIR/.git" ]]; then
    warn "Directory '$TARGET_DIR' is already a git repo."
    warn "Fetching latest changes instead of cloning..."
    git -C "$TARGET_DIR" fetch origin "$BRANCH"
    git -C "$TARGET_DIR" checkout "$BRANCH"
    git -C "$TARGET_DIR" pull origin "$BRANCH"
    success "Repository updated."
elif [[ -d "$TARGET_DIR" && -n "$(ls -A "$TARGET_DIR" 2>/dev/null)" ]]; then
    error "Directory '$TARGET_DIR' exists and is not empty. Choose a different name:"
    error "  bash setup.sh <new-directory-name>"
    exit 1
else
    git clone \
        --branch "$BRANCH" \
        --single-branch \
        --depth 1 \
        "$REPO_URL" \
        "$TARGET_DIR"
    success "Repository cloned into '$TARGET_DIR'."
fi

FULL_PATH="$(cd "$TARGET_DIR" && pwd)"

# ── 3. Verify project structure ───────────────────────────────────────────────
step "Verifying project structure"

REQUIRED_PATHS=(
    "Assets/Scripts"
    "Assets/Scenes"
    "Assets/Resources/QuizData"
    "Packages/manifest.json"
    "ProjectSettings/ProjectSettings.asset"
)

ALL_OK=true
for p in "${REQUIRED_PATHS[@]}"; do
    if [[ -e "$TARGET_DIR/$p" ]]; then
        success "$p"
    else
        error "Missing: $p"
        ALL_OK=false
    fi
done

if [[ "$ALL_OK" == false ]]; then
    error "Project structure is incomplete. The clone may have failed."
    exit 1
fi

# Count scripts
SCRIPT_COUNT=$(find "$TARGET_DIR/Assets/Scripts" -name "*.cs" | wc -l | tr -d ' ')
success "C# scripts found: $SCRIPT_COUNT"

# ── 4. Open in Unity Hub (if available) ───────────────────────────────────────
step "Unity Hub integration"

OPENED=false

if [[ "$UNITY_HUB_FOUND" == true ]]; then
    echo ""
    read -rp "  Open the project in Unity Hub now? [Y/n] " answer
    answer="${answer:-Y}"

    if [[ "$answer" =~ ^[Yy]$ ]]; then
        if command -v unityhub &>/dev/null; then
            unityhub -- --headless open "$FULL_PATH" &>/dev/null &
            OPENED=true
        elif [[ -d "/Applications/Unity Hub.app" ]]; then
            open -a "Unity Hub" "$FULL_PATH"
            OPENED=true
        fi

        if [[ "$OPENED" == true ]]; then
            success "Project sent to Unity Hub."
        else
            warn "Could not open automatically. Add manually (see step 5 below)."
        fi
    fi
else
    warn "Unity Hub not found — open the project manually."
fi

# ── 5. Print next steps ───────────────────────────────────────────────────────
step "Next Steps"

STEP=1
echo ""
if [[ "$OPENED" == false ]]; then
    echo -e "  ${BOLD}$((STEP++)). Open in Unity Hub${RESET}"
    echo "     Unity Hub → Projects → Add → select:"
    echo -e "     ${CYAN}$FULL_PATH${RESET}"
    echo ""
fi

echo -e "  ${BOLD}$((STEP++)). Set Unity version${RESET}"
echo "     If prompted, select Unity ${UNITY_VERSION}.x LTS"
echo "     (Unity Hub → Installs → Install Editor if not present)"
echo ""

echo -e "  ${BOLD}$((STEP++)). Wait for package resolution${RESET}"
echo "     Unity will auto-download AR Foundation 5.1.2, ARCore/ARKit,"
echo "     TextMeshPro, and other packages (~2–5 min first time)"
echo ""

echo -e "  ${BOLD}$((STEP++)). Build the scene hierarchy${RESET}"
echo "     Menu bar → ${BOLD}AR Education > Setup All Scenes${RESET}"
echo "     This creates all 4 scenes with full UI and component wiring."
echo ""

echo -e "  ${BOLD}$((STEP++)). Enable AR plugins${RESET}"
echo "     Edit → Project Settings → XR Plug-in Management"
echo "     → Android tab → check ${BOLD}ARCore${RESET}"
echo "     → iOS tab     → check ${BOLD}ARKit${RESET} (optional)"
echo ""

echo -e "  ${BOLD}$((STEP++)). Test in Editor${RESET}"
echo "     Open Assets/Scenes/Quiz.unity → Press Play"
echo "     Open Assets/Scenes/TeacherDashboard.unity → Press Play"
echo ""

echo -e "  ${BOLD}$((STEP++)). Build for Android${RESET}"
echo "     File → Build Settings → Android → Switch Platform"
echo "     Player Settings:"
echo "       • Bundle ID : com.areducation.mvp"
echo "       • Min SDK   : 24  (Android 7.0)"
echo "       • Backend   : IL2CPP"
echo "       • Arch      : ARM64"
echo "     → Build and Run (device must support ARCore)"
echo ""

# ── 6. Summary ────────────────────────────────────────────────────────────────
step "Setup Complete"
echo ""
success "Project ready at: $FULL_PATH"
echo ""
echo -e "  Scenes   : MainMenu | ARLesson | Quiz | TeacherDashboard"
echo -e "  Scripts  : $SCRIPT_COUNT C# files across AR / Lessons / Quiz / Teacher / UI / Data / Utils"
echo -e "  Quiz data: triangle_quiz.json | physics_quiz.json | cube_quiz.json"
echo ""
