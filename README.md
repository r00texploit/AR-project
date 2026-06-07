# AR Education

Production-oriented Android AR classroom app for student-owned ARCore devices. The app keeps student profile data, quiz history, diagnostics, and exported reports on the device only. It does not include backend sync, accounts, analytics, telemetry, or cloud storage.

## What It Does

Students choose a lesson, place the 3D lesson model in AR, interact with the model, complete quizzes, and export a local PDF progress report.

| Feature | Description |
|---|---|
| AR Triangle Lesson | Place a 3D triangle, adjust side lengths, and see perimeter/area update live |
| AR Physics Lesson | Launch a moving ball and inspect `d = v * t` with speed/time controls |
| AR Cube Lesson | Explore cube faces, size, surface area, and volume |
| Quiz System | Multiple-choice quizzes with immediate feedback and saved local attempts |
| Progress & Reports | Student profile, history filters, averages, clear data, and PDF export/share |
| Local Diagnostics | Rolling on-device log for troubleshooting without telemetry |

## Requirements

| Tool | Version | Notes |
|---|---|---|
| Unity | **6000.4.8f1** | CI-pinned production build version |
| Android Build Support | Unity module | Includes Android SDK, NDK, and OpenJDK |
| AR Foundation | 6.4.3 | Installed from `Packages/manifest.json` |
| ARCore XR Plugin | 6.4.3 | Required Android AR runtime |
| Android device | API 24+ | Must support ARCore |

## Build

Open the project in Unity, run `AR Education > Setup All Scenes`, then build Android.

Production Android settings:

| Setting | Value |
|---|---|
| App Name | `AR Education` |
| Bundle ID | `com.areducation.app` |
| Min SDK | Android 7.0 / API 24 |
| Target SDK | 34 |
| Scripting Backend | IL2CPP |
| Architecture | ARM64 |
| Required permissions | Camera only |
| ARCore | Required |

GitHub Actions builds a signed APK artifact from `.github/workflows/build-android.yml`. Configure these repository secrets before release builds:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `ANDROID_KEYSTORE_BASE64`
- `ANDROID_KEYSTORE_PASS`
- `ANDROID_KEYALIAS_NAME`
- `ANDROID_KEYALIAS_PASS`

The WebGL workflow is optional and artifact-only; Android is the release gate.

## Data & Privacy

All production data is local to the device:

- Student profile: stable `studentId`, name, grade level, class name, creation time
- Quiz attempts: attempt ID, lesson, score, timestamp, duration, app version
- Reports: generated PDF files under the app persistent data path
- Diagnostics: local rolling log only

Sample/mock student results are not merged into real progress by default. Development builds can explicitly load sample results for demos.

## Verification

EditMode tests cover:

- Quiz JSON shape and valid answer indices
- `LocalDataStore` empty, valid, and corrupt JSON handling
- Triangle and cube mesh generation
- Minimal PDF generation with student/result text

Manual Android QA checklist:

- Fresh install and profile setup
- Camera permission allow and deny paths
- ARCore unsupported-device message
- Place, reset/reposition, move, scale, and rotate each lesson
- Complete quizzes and verify persistence after reopening
- Export/share PDF report
- Clear data and confirm sample data does not return

## License

MIT. See [LICENSE](LICENSE).
