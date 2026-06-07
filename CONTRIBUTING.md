# Contributing Guide

How to extend AR Education with new lessons, quiz questions, scenes, and production-safe local features.

---

## Table of Contents

1. [Adding a New Lesson](#1-adding-a-new-lesson)
2. [Adding Quiz Questions](#2-adding-quiz-questions)
3. [Adding a New Scene](#3-adding-a-new-scene)
4. [Adding Student Data](#4-adding-student-data)
5. [Code Style](#5-code-style)
6. [Commit Conventions](#6-commit-conventions)
7. [Project File Rules](#7-project-file-rules)

---

## 1. Adding a New Lesson

### Step 1 — Add the enum value

`Assets/Scripts/Lessons/LessonManager.cs`:

```csharp
public enum LessonType { Triangle, Cube, Physics, Circle }  // ← add here
```

### Step 2 — Create the controller

Create `Assets/Scripts/Lessons/CircleLessonController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.Lessons
{
    public class CircleLessonController : LessonBase
    {
        [SerializeField] private Slider   radiusSlider;
        [SerializeField] private TMP_Text labelCircumference;
        [SerializeField] private TMP_Text labelArea;
        [SerializeField] private TMP_Text labelFormula;

        private float _radius = 1f;

        protected override void OnInitialize()
        {
            LessonId = "circle";
            radiusSlider.minValue = 0.5f;
            radiusSlider.maxValue = 8f;
            radiusSlider.value    = _radius;
            radiusSlider.onValueChanged.AddListener(v => { _radius = v; UpdateLabels(); });
            UpdateLabels();
        }

        protected override void OnReset()
        {
            _radius = 1f;
            if (radiusSlider) radiusSlider.value = _radius;
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            float c = 2f * Mathf.PI * _radius;
            float a = Mathf.PI * _radius * _radius;
            if (labelCircumference) labelCircumference.text = $"C = {c:F2}";
            if (labelArea)          labelArea.text          = $"A = {a:F2}";
            if (labelFormula)       labelFormula.text       = $"C = 2πr = 2π×{_radius:F1}";
        }
    }
}
```

### Step 3 — Wire it in LessonManager

```csharp
[SerializeField] private CircleLessonController circleLesson;  // add field

// in LoadLesson():
LessonType.Circle => circleLesson,
```

Also add to `HideAll()`:
```csharp
circleLesson?.Hide();
```

### Step 4 — Add a 3D object builder in the Editor script

In `Assets/Editor/AREducationSceneSetup.cs`, add a method:

```csharp
private static GameObject BuildCircleLessonObject(Transform parent)
{
    var root = new GameObject("CircleLesson");
    root.transform.SetParent(parent, false);
    // ... add MeshFilter, MeshRenderer, ARObjectManipulator
    root.AddComponent<CircleLessonController>();
    root.SetActive(false);
    return root;
}
```

Then call it inside `SetupARLessonScene()` alongside the other lesson builders, and wire the SerializedField via `SetField(lm, "circleLesson", circleLesson)`.

### Step 5 — Add a control panel in the HUD

Inside `SetupARLessonScene()`, build a control panel (copy the Triangle panel pattern) and wire it through `ARLessonHUDController` via `SetField`.

### Step 6 — Add selector buttons

In `MainMenuController` and `ARLessonHUDController`, add a button for the new lesson type following the existing Triangle/Cube/Physics pattern.

### Step 7 — Add a .meta file

Create `Assets/Scripts/Lessons/CircleLessonController.cs.meta`:

```yaml
fileFormatVersion: 2
guid: <generate a unique 32-char hex string>
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

---

## 2. Adding Quiz Questions

Quiz data lives in `Assets/Resources/QuizData/`. The file name pattern is `{lessonId}_quiz.json`.

### Existing files

| File | Lesson ID |
|---|---|
| `triangle_quiz.json` | `"triangle"` |
| `physics_quiz.json` | `"physics"` |
| `cube_quiz.json` | `"cube"` |

### Question format

```json
{
  "questions": [
    {
      "questionId": "circle_001",
      "lessonId": "circle",
      "questionText": "What is the circumference of a circle with radius 7?",
      "options": ["21.99", "43.98", "153.94", "14"],
      "correctIndex": 1,
      "explanation": "C = 2πr = 2 × 3.14159 × 7 ≈ 43.98"
    }
  ]
}
```

Rules:
- `options` must have exactly **4** elements
- `correctIndex` is 0-based (0–3)
- `explanation` is shown after the student answers
- `lessonId` should match the filename prefix

### Adding a new quiz file

1. Create `Assets/Resources/QuizData/circle_quiz.json` with the format above
2. Create the corresponding `circle_quiz.json.meta` file (use `TextScriptImporter`)
3. In `MainMenuController` and `QuizUIController`, add a button that calls `LaunchQuiz("circle")`

---

## 3. Adding a New Scene

1. Create the scene file at `Assets/Scenes/MyScene.unity` (copy any existing skeleton)
2. Create `Assets/Scenes/MyScene.unity.meta` with a unique GUID
3. Add a builder method in `AREducationSceneSetup.cs`:

```csharp
[MenuItem("AR Education/Setup My Scene")]
public static void SetupMyScene()
{
    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    // build GameObjects...
    EditorSceneManager.SaveScene(scene, "Assets/Scenes/MyScene.unity");
}
```

4. Add it to `SetupBuildSettings()`:

```csharp
new EditorBuildSettingsScene("Assets/Scenes/MyScene.unity", true),
```

5. Add a constant to `SceneLoader.cs`:

```csharp
public const int SceneMyScene = 4;
```

---

## 4. Adding Student Data

Mock students and pre-seeded results live in `Assets/Resources/StudentData/sample_students.json`.

```json
{
  "students": [
    { "studentId": "s006", "studentName": "Fatima Al-Rashid", "gradeLevel": "Grade 9" }
  ],
  "quizResults": [
    {
      "studentId": "s006",
      "studentName": "Fatima Al-Rashid",
      "lessonId": "triangle",
      "lessonTitle": "Triangle Lesson",
      "score": 5,
      "totalQuestions": 5,
      "percentage": 100.0,
      "timestamp": "2024-02-01T09:00:00"
    }
  ]
}
```

`DataManager.GetAllResults()` merges this file with real PlayerPrefs results automatically.

---

## 5. Code Style

- **Namespaces**: All scripts use `namespace AREducation.<Layer>` (e.g. `AREducation.Lessons`)
- **Null-safety**: Use `?.` for optional SerializeField references — never assume they are assigned
- **Events**: Unsubscribe in `OnDestroy()` to prevent memory leaks across scene loads
- **SerializedFields**: Use `[SerializeField] private` — not `public`
- **No comments on obvious code**: Only comment non-obvious invariants or workarounds
- **No magic strings**: Lesson IDs (`"triangle"`, `"physics"`, `"cube"`) should match the JSON filename prefix exactly

---

## 6. Commit Conventions

Use the `feat / fix / refactor / docs / chore` prefix:

```
feat: Add circle lesson with circumference slider
fix: Prevent AR placement when tapping UI buttons
refactor: Extract face-index logic into MeshGenerator
docs: Add circle lesson to CONTRIBUTING.md
chore: Add CircleLessonController.cs.meta
```

---

## 7. Project File Rules

| Rule | Reason |
|---|---|
| Every `.cs`, `.json`, `.unity` file needs a `.meta` sibling | Unity uses GUIDs from meta files to track asset references |
| Every new folder needs a `.meta` file | Same as above |
| GUIDs must be unique 32-char hex strings | Duplicate GUIDs cause asset-reference corruption |
| Scene files must be added to `EditorBuildSettings` | Otherwise `SceneLoader.LoadScene(int)` fails silently |
| JSON resource files must be inside `Assets/Resources/` | `Resources.Load<TextAsset>()` only searches that path |
| `link.xml` must list namespaces of any new serialisable classes | Prevents IL2CPP from stripping them in release builds |
