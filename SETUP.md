# Setup Guide

Complete installation and configuration instructions for the AR Education MVP.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Clone the Repository](#2-clone-the-repository)
3. [Open in Unity Hub](#3-open-in-unity-hub)
4. [Package Resolution](#4-package-resolution)
5. [Build Scene Hierarchy](#5-build-scene-hierarchy)
6. [Configure XR Plugin Management](#6-configure-xr-plugin-management)
7. [Import TextMeshPro Resources](#7-import-textmeshpro-resources)
8. [Test in Editor](#8-test-in-editor)
9. [Android Build](#9-android-build)
10. [iOS Build](#10-ios-build)
11. [Troubleshooting](#11-troubleshooting)

---

## 1. Prerequisites

| Tool | Version | Download |
|---|---|---|
| Unity Hub | Latest | https://unity.com/download |
| Unity Editor | **2022.3 LTS** | Via Unity Hub → Installs |
| Android Build Support | (module) | Via Unity Hub alongside the Editor |
| Android SDK / NDK | Bundled | Installed automatically with Android module |
| Git | Any | https://git-scm.com/downloads |

### Unity modules to install

When installing Unity 2022.3 LTS via Unity Hub, check these add-on modules:

- **Android Build Support**
  - Android SDK & NDK Tools
  - OpenJDK
- **iOS Build Support** (optional)

---

## 2. Clone the Repository

### Using the setup script (recommended)

**Linux / macOS:**
```bash
bash <(curl -fsSL https://raw.githubusercontent.com/r00texploit/AR-project/claude/ar-education-mvp-CWdbk/setup.sh)
```

**Windows PowerShell:**
```powershell
irm https://raw.githubusercontent.com/r00texploit/AR-project/claude/ar-education-mvp-CWdbk/setup.ps1 | iex
```

### Manual clone

```bash
git clone \
  --branch claude/ar-education-mvp-CWdbk \
  --single-branch \
  https://github.com/r00texploit/AR-project.git \
  AR-Education-MVP

cd AR-Education-MVP
```

---

## 3. Open in Unity Hub

1. Launch **Unity Hub**
2. Click **Projects** → **Add** → **Add project from disk**
3. Navigate to the cloned folder and select it
4. Unity Hub will detect the project — if prompted to select an editor version, choose **2022.3.x LTS**
5. Click the project name to open it

> **First open takes 3–10 minutes** while Unity compiles scripts and imports packages.

---

## 4. Package Resolution

Unity automatically downloads all packages listed in `Packages/manifest.json`:

| Package | Version | Purpose |
|---|---|---|
| `com.unity.xr.arfoundation` | 5.1.2 | AR Foundation core |
| `com.unity.xr.arcore` | 5.1.2 | Android AR backend |
| `com.unity.xr.arkit` | 5.1.2 | iOS AR backend |
| `com.unity.xr.management` | 4.4.0 | XR loader management |
| `com.unity.xr.core-utils` | 2.2.3 | XROrigin component |
| `com.unity.textmeshpro` | 3.0.6 | Text rendering |
| `com.unity.nuget.newtonsoft-json` | 3.2.1 | JSON library |
| `com.unity.inputsystem` | 1.7.0 | New Input System |

No manual package installation is needed. If you see red errors on first open, wait for the package download to complete and Unity will recompile.

---

## 5. Build Scene Hierarchy

The scene `.unity` files are minimal skeletons. The Editor script creates the full GameObjects, components, and wired references:

1. In the Unity menu bar click: **AR Education → Setup All Scenes**
2. Unity will create and save all four scenes:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/ARLesson.unity`
   - `Assets/Scenes/Quiz.unity`
   - `Assets/Scenes/TeacherDashboard.unity`
3. A dialog confirms success and shows the next steps

> If the menu item is not visible, check the **Console** window for compilation errors first. Fix any errors before running setup.

---

## 6. Configure XR Plugin Management

AR Foundation needs a platform-specific loader enabled:

1. **Edit → Project Settings → XR Plug-in Management**
2. On the **Android** tab:
   - Check **ARCore**
3. On the **iOS** tab (optional):
   - Check **ARKit**
4. Close Project Settings

Unity will show a progress bar while configuring the XR subsystem.

---

## 7. Import TextMeshPro Resources

If you see placeholder text or pink UI elements:

1. **Window → TextMeshPro → Import TMP Essential Resources**
2. Click **Import** in the dialog

This only needs to be done once per project.

---

## 8. Test in Editor

Some scenes work in the Editor without an AR device:

| Scene | Editor playable? | Notes |
|---|---|---|
| `MainMenu.unity` | Yes | Test navigation, settings, student name |
| `ARLesson.unity` | Partial | AR camera feed requires device; lesson UI works in simulator |
| `Quiz.unity` | Yes | Full quiz flow — answer questions, see score |
| `TeacherDashboard.unity` | Yes | Mock student data loads from Resources/ |

**To play a scene:**
1. `File → Open Scene` → select scene
2. Press the **Play** button (▶)

---

## 9. Android Build

### One-time setup

1. **File → Build Settings**
2. Select **Android** and click **Switch Platform**
3. Click **Player Settings** and verify:

| Setting | Value |
|---|---|
| Product Name | AR Education MVP |
| Bundle Identifier | `com.areducation.mvp` |
| Minimum API Level | Android 7.0 (API 24) |
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 |
| Internet Access | Auto |

4. Back in Build Settings, click **Add Open Scenes** — or add manually:
   - `Assets/Scenes/MainMenu.unity` (index 0)
   - `Assets/Scenes/ARLesson.unity` (index 1)
   - `Assets/Scenes/Quiz.unity` (index 2)
   - `Assets/Scenes/TeacherDashboard.unity` (index 3)

### Build and run

- **Build** → saves an `.apk` file (sideload manually)
- **Build and Run** → installs directly to a connected USB-debugging-enabled device

> The device must support **ARCore**. Check the [supported devices list](https://developers.google.com/ar/devices).

---

## 10. iOS Build

1. **File → Build Settings → iOS → Switch Platform**
2. **Player Settings:**
   - Bundle ID: `com.areducation.mvp`
   - Camera Usage Description: `Required for Augmented Reality lessons`
   - Target minimum iOS version: `13.0`
3. **Build** → generates an Xcode project folder
4. Open the `.xcodeproj` in Xcode
5. Set your Apple Developer Team under Signing & Capabilities
6. Connect an iOS device and click **Run**

> Requires a Mac with Xcode 14+. The device must support ARKit (iPhone 6s or newer).

---

## 11. Troubleshooting

### Compilation errors on first open

**Cause:** Package downloads are still in progress.  
**Fix:** Wait for the progress bar in the bottom-right to finish, then Unity recompiles.

### `AR Education` menu not visible

**Cause:** Compilation errors preventing Editor scripts from loading.  
**Fix:** Open **Window → General → Console** and resolve all red errors first.

### Pink / missing UI text

**Cause:** TextMeshPro Essential Resources not imported.  
**Fix:** **Window → TextMeshPro → Import TMP Essential Resources**

### "AR is not supported on this device" message at runtime

**Cause:** ARCore not installed or device not supported.  
**Fix:** Install Google Play Services for AR from the Play Store, or test on a [supported device](https://developers.google.com/ar/devices).

### Scenes appear empty after `Setup All Scenes`

**Cause:** Script compilation error prevented the Editor script from completing.  
**Fix:** Check the Console for errors, fix them, then re-run **AR Education → Setup All Scenes**.

### Quiz data not loading

**Cause:** JSON files not in a `Resources/` folder or file name mismatch.  
**Fix:** Confirm these paths exist:
```
Assets/Resources/QuizData/triangle_quiz.json
Assets/Resources/QuizData/physics_quiz.json
Assets/Resources/QuizData/cube_quiz.json
```

### Android build fails: "No valid Android SDK found"

**Fix:** Unity Hub → Installs → your Unity version → three dots (⋮) → Add Modules → Android Build Support + Android SDK & NDK.

### IL2CPP build fails with "TypeLoadException" at runtime

**Cause:** IL2CPP stripped a serializable class.  
**Fix:** `Assets/link.xml` already handles this. If you add new model classes, add their namespace to `link.xml`.
