# Setup Guide

Installation, local verification, and Android release setup for **AR Education**.

## Prerequisites

| Tool | Version |
|---|---|
| Unity Hub | Latest |
| Unity Editor | **6000.4.8f1** for CI parity |
| Android Build Support | Installed with SDK, NDK, and OpenJDK modules |
| Git | Any current version |

The target platform is Android ARCore. iOS and WebGL are not production release targets in this pass.

## Open The Project

1. Clone the repository.
2. Open Unity Hub.
3. Add `/Users/halim/AR-project-main` or your cloned project folder.
4. Open the project with Unity `6000.4.8f1`.
5. Wait for package import and script compilation to finish.

## Generate Scenes

The scene files are generated from the editor setup script so references stay reproducible.

1. In Unity, choose `AR Education > Setup All Scenes`.
2. Confirm these scenes are saved:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/ARLesson.unity`
   - `Assets/Scenes/Quiz.unity`
   - `Assets/Scenes/TeacherDashboard.unity`

`TeacherDashboard.unity` now hosts the student-facing **Progress & Reports** flow.

## Android Configuration

Verify these settings before a release build:

| Setting | Value |
|---|---|
| Product Name | `AR Education` |
| Package Name | `com.areducation.app` |
| Minimum API Level | Android 7.0 / API 24 |
| Target API Level | 34 |
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 |
| Orientation | Portrait |
| Required permissions | Camera |
| ARCore | Required |
| Backup | Disabled |

The manifest intentionally does not request `WRITE_EXTERNAL_STORAGE` or `INTERNET`.

## Build Locally

1. Connect an ARCore-capable Android device with USB debugging enabled.
2. In Unity, open `File > Build Settings`.
3. Select Android and click `Switch Platform`.
4. Add the four scenes listed above in order.
5. Click `Build` or `Build And Run`.

## GitHub Actions Release Build

Android release builds run through `.github/workflows/build-android.yml` using Unity `6000.4.8f1`.

Required Unity secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Required signing secrets:

- `ANDROID_KEYSTORE_BASE64`
- `ANDROID_KEYSTORE_PASS`
- `ANDROID_KEYALIAS_NAME`
- `ANDROID_KEYALIAS_PASS`

The workflow uploads a signed APK named like `AR-Education-v1.0-build123.apk`.

## Tests

Run EditMode tests locally:

```bash
/Applications/Unity/Hub/Editor/6000.4.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -automated \
  -projectPath /Users/halim/AR-project-main \
  -runTests -testPlatform EditMode -runSynchronously \
  -testResults /tmp/ar_editmode_results.xml \
  -logFile /tmp/ar_editmode.log
```

CI also runs `.github/workflows/unity-smoke-tests.yml` for EditMode smoke coverage.

## Manual Android QA

- Fresh install and update student profile.
- Deny camera permission and confirm the app explains the blocked AR state.
- Allow camera permission and verify ARCore availability handling.
- Place, reset/reposition, move, scale, and rotate each lesson object.
- Complete all quizzes.
- Export/share a PDF report.
- Reopen the app and confirm quiz history persists.
- Clear results and confirm sample data does not reappear.
