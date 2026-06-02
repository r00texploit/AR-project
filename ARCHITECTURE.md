# Architecture

Technical overview of the AR Education MVP codebase — design decisions, component communication patterns, and data flow.

---

## Table of Contents

1. [High-Level Architecture](#1-high-level-architecture)
2. [Scene Architecture](#2-scene-architecture)
3. [Script Layer Overview](#3-script-layer-overview)
4. [Data Layer](#4-data-layer)
5. [AR Layer](#5-ar-layer)
6. [Lesson System](#6-lesson-system)
7. [Quiz System](#7-quiz-system)
8. [Teacher Dashboard](#8-teacher-dashboard)
9. [UI Layer](#9-ui-layer)
10. [Utilities](#10-utilities)
11. [Component Communication](#11-component-communication)
12. [Android / iOS Configuration](#12-android--ios-configuration)

---

## 1. High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Scenes (4)                          │
│  MainMenu ── ARLesson ── Quiz ── TeacherDashboard       │
└────────────────────┬────────────────────────────────────┘
                     │ SceneLoader (DontDestroyOnLoad)
┌────────────────────▼────────────────────────────────────┐
│                  Managers (Singletons)                   │
│  DataManager  ·  QuizManager  ·  LessonManager          │
└────────────┬───────────────────────────┬────────────────┘
             │                           │
┌────────────▼──────────┐   ┌────────────▼───────────────┐
│      AR Layer         │   │       Data Layer            │
│  ARSessionController  │   │  PlayerPrefs (JSON)         │
│  ARPlacementManager   │   │  Resources/ (JSON files)    │
│  ARObjectManipulator  │   └────────────────────────────┘
└────────────┬──────────┘
┌────────────▼──────────────────────────────────────────┐
│              Lesson Controllers                        │
│  TriangleLessonController  (cosine rule mesh + Heron)  │
│  PhysicsLessonController   (d = v × t simulation)     │
│  CubeLessonController      (face tap + V = s³)        │
└───────────────────────────────────────────────────────┘
```

---

## 2. Scene Architecture

The project uses a **Scene-per-Feature** layout with a shared persistent singleton layer.

| Build Index | Scene | Responsibilities |
|---|---|---|
| 0 | `MainMenu` | Navigation hub, student name entry, lesson/quiz selection |
| 1 | `ARLesson` | AR Foundation session, plane detection, lesson object placement |
| 2 | `Quiz` | Question flow, scoring, result display |
| 3 | `TeacherDashboard` | Student result list with lesson filter |

### Persistent GameObjects

`DataManager` and `SceneLoader` are created in `MainMenu` with `DontDestroyOnLoad`. Every other scene can access them via their `Instance` properties without needing scene-level prefabs.

```
MainMenu scene loads
  → creates DataManager (DontDestroyOnLoad)
  → creates SceneLoader  (DontDestroyOnLoad)
     ↓
Any scene change
  → SceneLoader.LoadScene(int) → async load → activate
  → DataManager.Instance remains available
```

---

## 3. Script Layer Overview

```
Assets/Scripts/
├── Data/
│   ├── DataModels.cs           [System.Serializable] POCOs
│   └── DataManager.cs          Singleton, PlayerPrefs persistence
├── AR/
│   ├── ARSessionController.cs  Permission request, state events
│   ├── ARPlacementManager.cs   Plane raycast, tap-to-place
│   └── ARObjectManipulator.cs  Touch gestures (drag/scale/rotate)
├── Lessons/
│   ├── LessonBase.cs           Abstract: Initialize / Reset / Show / Hide
│   ├── LessonManager.cs        Activates the selected lesson, wires placement
│   ├── TriangleLessonController.cs
│   ├── PhysicsLessonController.cs
│   └── CubeLessonController.cs
├── Quiz/
│   ├── QuizManager.cs          Loads JSON, checks answers, fires events
│   └── QuizUIController.cs     Subscribes to events, drives UI
├── Teacher/
│   ├── TeacherDashboardController.cs  Loads/filters/displays results
│   └── ResultRowUI.cs                 One row in the scroll list
├── UI/
│   ├── MainMenuController.cs   Scene navigation, panel toggles
│   └── ARLessonHUDController.cs In-AR overlay: hint, controls, switcher
└── Utils/
    ├── MeshGenerator.cs        Procedural triangle (prism) + cube
    └── SceneLoader.cs          Async scene transitions
```

---

## 4. Data Layer

### Models (`DataModels.cs`)

All models use `[System.Serializable]` for Unity's `JsonUtility`. Wrapper classes (`QuizQuestionList`, `QuizResultList`) exist because `JsonUtility` cannot deserialise a root JSON array.

```csharp
StudentProfile     { studentId, studentName, gradeLevel }
QuizQuestion       { questionId, lessonId, questionText, options[], correctIndex, explanation }
QuizResult         { studentId, studentName, lessonId, lessonTitle, score, totalQuestions,
                     percentage, timestamp }
QuizResultList     { List<QuizResult> results }   // JsonUtility wrapper
MockStudentData    { StudentProfile[] students, QuizResult[] quizResults }
```

### Persistence (`DataManager.cs`)

```
Save path:   PlayerPrefs key "quiz_results_v1"
Format:      JsonUtility.ToJson(QuizResultList)
Mock data:   Resources.Load<TextAsset>("StudentData/sample_students")
```

Results from `GetAllResults()` merge the mock JSON with real PlayerPrefs data so the teacher dashboard always has demo content even on first run.

---

## 5. AR Layer

### ARSessionController

Handles the AR startup sequence:

```
Start()
  ├─ Android: Permission.RequestUserPermission(Camera)
  ├─ ARSession.CheckAvailability()
  ├─ availability == NotSupported → show error panel + fire OnARNotSupported
  └─ OK → hide init panel + fire OnARReady
```

Subscribes to `ARSession.stateChanged` to show/hide the "Scanning…" indicator.

### ARPlacementManager

```
SetPendingObject(GameObject)   ← called by LessonManager
EnablePlacement(bool)          ← called by ARSessionController via OnARReady
OnObjectPlaced (UnityEvent<GameObject, Pose>)

Update():
  touchCount == 1, phase == Began, not over UI
    → raycastManager.Raycast(pos, hits, PlaneWithinPolygon)
    → place object at hits[0].pose
    → HidePlaneVisuals() — disables ARPlaneManager + all plane trackables
    → fire OnObjectPlaced
```

UI touch guard uses `EventSystem.IsPointerOverGameObject(fingerId)` to prevent placement when tapping UI elements.

### ARObjectManipulator

Attached to each placed lesson root object.

| Gesture | Action |
|---|---|
| 1 finger drag | Translate along horizontal plane (camera right/up vectors, Y clamped to 0) |
| 2 finger pinch | Scale uniformly, clamped `[0.15, 4.0]` |
| 2 finger twist | Rotate around world Y-axis |

Rotation uses `Space.World` + `Vector3.up` so the object always spins around the vertical axis regardless of its local orientation.

---

## 6. Lesson System

### LessonBase (Abstract)

Template Method pattern — subclasses implement two abstract methods:

```csharp
protected abstract void OnInitialize();  // called once on first show
protected abstract void OnReset();       // called by reset button
```

Public API: `Initialize()`, `ResetLesson()`, `Show()`, `Hide()`, `OnPlaced(Pose)`.

### LessonManager

Reads the static field `LessonManager.SelectedLesson` (set by `MainMenuController` before scene load) and activates the matching controller.

```
Start() → HideAll() → LoadLesson(SelectedLesson)
LoadLesson(type):
  currentLesson = triangleLesson | cubeLesson | physicsLesson
  currentLesson.Initialize()
  placementManager.SetPendingObject(currentLesson.gameObject)
```

### TriangleLessonController

Key maths:

```csharp
// Vertex positions (sides: a=BC, b=CA, c=AB)
// Place A at origin, B at (c, 0, 0)
// Angle at A: cos(A) = (b²+c²-a²) / (2bc)
float cosA = (b*b + c*c - a*a) / (2*b*c);
Vector3 vC = new Vector3(b * cosA, 0, b * Mathf.Sqrt(1 - cosA*cosA));

// Heron's formula
float s    = (a + b + c) / 2f;
float area = Mathf.Sqrt(s * (s-a) * (s-b) * (s-c));
```

The mesh is a triangular prism (extruded 0.04 units) for 3D appearance. A `LineRenderer` traces the perimeter outline. Slider callbacks validate the triangle inequality before rebuilding the mesh.

### PhysicsLessonController

Pure kinematic simulation — no Unity Rigidbody involved:

```csharp
void Update() {
    if (!isMoving) return;
    elapsed   += Time.deltaTime;
    distance   = speed * elapsed;           // d = v × t
    ball.localPosition = start + Vector3.right * distance * 0.1f;
    if (distance >= maxDistance) ResetBall();
}
```

Speed is clamped `[0.5, 10.0]` m/s in steps of 0.5. A `TrailRenderer` provides visual motion feedback.

### CubeLessonController

Face identification uses the hit normal in local space:

```csharp
// Transform world hit normal to local space
Vector3 localNorm = transform.InverseTransformDirection(hit.normal).normalized;
// Dot product against each face's canonical normal → pick highest dot
int face = FaceNormals.Select((n,i)=>(dot:Vector3.Dot(localNorm,n),i))
                      .OrderByDescending(t=>t.dot).First().i;
```

---

## 7. Quiz System

### Event Contract

`QuizManager` exposes three `Action<>` events. `QuizUIController` subscribes and updates the UI purely in response to these events — no polling.

```
OnQuestionChanged(QuizQuestion q, int index, int total)
  → display question text, 4 option labels, progress "Q 2/5"

OnAnswerChecked(bool correct, string explanation, int currentScore)
  → colour buttons green/red, show explanation, enable Next button

OnQuizComplete(int finalScore, int total)
  → hide quiz panel, show result panel with grade
```

Event subscriptions are cleaned up in `OnDestroy()` to prevent null-reference exceptions during scene transitions.

### Question Loading

```csharp
TextAsset asset = Resources.Load<TextAsset>($"QuizData/{lessonId}_quiz");
QuizQuestionList data = JsonUtility.FromJson<QuizQuestionList>(asset.text);
```

Questions are shuffled (Fisher-Yates) before each quiz run to add replay variety.

### Score Saving

On quiz completion, `QuizManager` calls `DataManager.Instance.SaveQuizResult(...)` with the current student name from `DataManager.GetStudentName()`.

---

## 8. Teacher Dashboard

```
Start()
  → DataManager.GetAllResults()          // mock + real merged
  → ApplyFilter("")                      // show all
  → PopulateList(results)
       → clear Content transform
       → foreach result: Instantiate(resultRowPrefab)
       → ResultRowUI.Setup(result)

OnFilterChanged(lessonId)
  → filter _allResults by lessonId
  → PopulateList(filtered)
  → update summaryText + averageText
```

The `ScrollView` uses `VerticalLayoutGroup` + `ContentSizeFitter` on the Content transform so rows stack automatically.

---

## 9. UI Layer

### MainMenuController

Uses panel show/hide (no scene change) for the lesson/quiz selector and settings panels. Scene transitions go through `SceneLoader.Instance.LoadScene(int)`.

Static fields set before scene load:

```csharp
LessonManager.SelectedLesson   = LessonType.Triangle;  // before ARLesson load
QuizManager.SelectedLessonId   = "triangle";           // before Quiz load
```

### ARLessonHUDController

Manages three mutually exclusive control panels (Triangle / Physics / Cube). Listens to `ARPlacementManager.OnObjectPlaced` to hide the placement hint and show the active lesson panel.

```
OnObjectPlaced(go, pose)
  → placementHintPanel.SetActive(false)
  → ShowControlsForCurrentLesson()
  → _controlsVisible = true
```

The toggle button switches `_controlsVisible` and shows/hides the panel without touching placement state.

---

## 10. Utilities

### MeshGenerator (static)

**Triangle** — `GenerateTriangle(a, b, c)`:
- Positions vertex C using the cosine rule
- Centres the triangle at the origin for stable rotation
- Applies AR scale (`× 0.1`; 1 lesson unit ≈ 10 cm)
- Extrudes into a prism for 3D depth
- Returns `(Mesh, vA, vB, vC)` — vertex positions used by `LineRenderer`

**Cube** — `GenerateCube(size)`:
- 24 vertices (4 per face, unshared) for correct per-face normals
- 36 triangle indices
- Used by both `CubeLessonController` and the Editor setup script

### SceneLoader

Wraps `SceneManager.LoadSceneAsync` with a loading canvas overlay. `allowSceneActivation = false` during load lets the progress bar reach 100% before the scene switches. Created in `MainMenu` as a `DontDestroyOnLoad` singleton.

---

## 11. Component Communication

```
MainMenuController
  ──static write──► LessonManager.SelectedLesson (enum)
  ──static write──► QuizManager.SelectedLessonId (string)
  ──method call──►  SceneLoader.Instance.LoadScene(int)
  ──method call──►  DataManager.Instance.Get/SetStudentName()

ARSessionController
  ──UnityEvent──► ARPlacementManager.EnablePlacement(true)  [OnARReady]

ARPlacementManager
  ──UnityEvent<GO,Pose>──► ARLessonHUDController.OnObjectPlaced()
  ──UnityEvent<GO,Pose>──► (any subscriber)

LessonManager
  ──direct ref──► TriangleLessonController.Initialize()
  ──direct ref──► ARPlacementManager.SetPendingObject()

TriangleLessonController
  ──Slider.onValueChanged──► UpdateMesh()  [internal]
  ──static call──────────►  MeshGenerator.GenerateTriangle()

QuizManager
  ──Action event──► QuizUIController.DisplayQuestion()    [OnQuestionChanged]
  ──Action event──► QuizUIController.ShowFeedback()       [OnAnswerChecked]
  ──Action event──► QuizUIController.ShowResults()        [OnQuizComplete]
  ──method call──►  DataManager.Instance.SaveQuizResult()

TeacherDashboardController
  ──method call──► DataManager.Instance.GetAllResults()
  ──Dropdown──────► ApplyFilter(lessonId)
```

---

## 12. Android / iOS Configuration

### Android

| File | Key Settings |
|---|---|
| `ProjectSettings.asset` | `AndroidMinSdkVersion: 24`, `scriptingBackend: IL2CPP`, `targetArchitectures: ARM64` |
| `AndroidManifest.xml` | `CAMERA` permission, `android.hardware.camera.ar` feature, `com.google.ar.core: required` |
| `link.xml` | Preserves `Assembly-CSharp` from IL2CPP stripping |

### iOS

| Setting | Value |
|---|---|
| Minimum iOS | 13.0 |
| Camera Usage Description | "Required for Augmented Reality lessons" |
| XR Plugin | ARKit (enabled in XR Plug-in Management) |

### Input System

`activeInputHandler: 2` (Both) in `ProjectSettings.asset` enables both the legacy `Input.GetTouch()` API (used in gesture code) and the new Input System package required by AR Foundation 5.x simultaneously.
