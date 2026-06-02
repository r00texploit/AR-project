# AR Education MVP

An interactive Augmented Reality educational app for students to learn **mathematics** and **physics** through hands-on 3D experiences — built with Unity 2022.3 LTS and AR Foundation 5.x.

---

## Overview

Students place 3D lesson objects in their real environment, interact with them using touch gestures, explore formulas visually, and take quizzes. Teachers can review quiz scores on a dashboard.

| Feature | Description |
|---|---|
| AR Lesson — Triangle | Place a 3D triangle, adjust sides A/B/C with sliders, watch the mesh update, see perimeter and area recalculated live |
| AR Lesson — Physics | Launch a ball in AR, control speed, observe `d = v × t` in real time |
| AR Lesson — Cube | Tap individual faces, resize with a slider, explore surface area and volume |
| Quiz System | 5-question multiple-choice quizzes per topic with immediate feedback and scoring |
| Teacher Dashboard | Scrollable list of all student results, filterable by lesson |

---

## Screenshots / Demo

> **Note:** Run `AR Education > Setup All Scenes` in Unity Editor first to generate the full UI hierarchy, then open each scene to see the result.

| Main Menu | AR Triangle Lesson | Quiz | Teacher Dashboard |
|---|---|---|---|
| Lesson/Quiz selector, Settings | Plane detection, place + interact | MCQ + score | Filter by lesson, average score |

---

## Requirements

| Tool | Version | Notes |
|---|---|---|
| Unity | **2022.3 LTS** | Install via Unity Hub |
| AR Foundation | 5.1.2 | Auto-installed from manifest.json |
| ARCore XR Plugin | 5.1.2 | Android support |
| ARKit XR Plugin | 5.1.2 | iOS support (optional) |
| TextMeshPro | 3.0.6 | Auto-installed |
| Android device | API 24+ (Android 7.0+) | Must support ARCore |
| iOS device | iOS 13+ | A9 chip or newer |

---

## Quick Start

### Linux / macOS

```bash
bash <(curl -fsSL https://raw.githubusercontent.com/r00texploit/AR-project/claude/ar-education-mvp-CWdbk/setup.sh)
```

### Windows (PowerShell)

```powershell
irm https://raw.githubusercontent.com/r00texploit/AR-project/claude/ar-education-mvp-CWdbk/setup.ps1 | iex
```

### Manual clone

```bash
git clone --branch claude/ar-education-mvp-CWdbk \
          --single-branch \
          https://github.com/r00texploit/AR-project.git
```

Then open the cloned folder in **Unity Hub** and follow the [Setup Guide](SETUP.md).

---

## Project Structure

```
AR-project/
├── Assets/
│   ├── Editor/
│   │   └── AREducationSceneSetup.cs    ← "AR Education > Setup All Scenes"
│   ├── Plugins/
│   │   └── Android/
│   │       └── AndroidManifest.xml     ← Camera permission, ARCore required
│   ├── Resources/
│   │   ├── QuizData/                   ← triangle/physics/cube _quiz.json
│   │   └── StudentData/                ← sample_students.json (mock data)
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── ARLesson.unity
│   │   ├── Quiz.unity
│   │   └── TeacherDashboard.unity
│   ├── Scripts/
│   │   ├── AR/                         ← Placement, manipulation, session
│   │   ├── Data/                       ← Models, persistence
│   │   ├── Lessons/                    ← Triangle, Physics, Cube controllers
│   │   ├── Quiz/                       ← Manager, UI controller
│   │   ├── Teacher/                    ← Dashboard, result row
│   │   ├── UI/                         ← Main menu, AR HUD
│   │   └── Utils/                      ← Mesh generator, scene loader
│   └── link.xml                        ← IL2CPP stripping protection
├── Packages/
│   └── manifest.json                   ← Unity package dependencies
├── ProjectSettings/
│   └── ProjectSettings.asset           ← Android SDK 24, IL2CPP, ARM64
├── setup.sh                            ← Linux/macOS setup script
├── setup.ps1                           ← Windows PowerShell setup script
├── README.md
├── SETUP.md
├── ARCHITECTURE.md
└── CONTRIBUTING.md
```

---

## Key Scripts

| Script | Purpose |
|---|---|
| `ARPlacementManager.cs` | Raycasts against AR planes, places lesson objects on tap |
| `ARObjectManipulator.cs` | 1-finger drag, 2-finger pinch-scale + twist-rotate |
| `TriangleLessonController.cs` | Cosine-rule mesh update, Heron's area, perimeter formula |
| `PhysicsLessonController.cs` | Ball motion simulation — `d = v × t` |
| `CubeLessonController.cs` | Face tap detection, volume/surface-area formulas |
| `MeshGenerator.cs` | Procedural triangle (prism) and cube meshes |
| `QuizManager.cs` | JSON-loaded questions, event-driven answer checking |
| `DataManager.cs` | Singleton, PlayerPrefs JSON persistence, mock data loader |
| `AREducationSceneSetup.cs` | Editor script — builds full scene hierarchy programmatically |

---

## Android Build

```
File → Build Settings → Android → Switch Platform
```

| Setting | Value |
|---|---|
| Bundle ID | `com.areducation.mvp` |
| Min SDK | 24 (Android 7.0) |
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 |
| XR Plugin | ARCore (enabled in XR Plug-in Management) |

---

## Data & Storage

All data is stored **locally** (no internet required for MVP):

- Quiz results → `PlayerPrefs` key `quiz_results_v1` (JSON array)
- Student name → `PlayerPrefs` key `student_name`
- Mock students + pre-seeded scores → `Resources/StudentData/sample_students.json`

---

## Roadmap

- [ ] Firebase Firestore sync for multi-device teacher dashboard
- [ ] User login / student profiles
- [ ] Additional lessons: fractions, algebra, circular motion
- [ ] Animated formula overlays (step-by-step)
- [ ] Teacher lesson assignment workflow
- [ ] Localization (Arabic, French)

---

## Documentation

| File | Contents |
|---|---|
| [SETUP.md](SETUP.md) | Detailed installation, Unity configuration, build steps |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Code design, component communication, data flow |
| [CONTRIBUTING.md](CONTRIBUTING.md) | How to add lessons, quizzes, and scenes |

---

## License

MIT — see [LICENSE](LICENSE) for details.
