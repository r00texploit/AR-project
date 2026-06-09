using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.AR;
using AREducation.Lessons;
using AREducation.Quiz;
using AREducation.Utils;
using UnityEngine.XR.ARFoundation;

namespace AREducation.UI
{
    /// <summary>
    /// Controls the HUD overlay during an AR lesson:
    /// placement hint, lesson controls panel, lesson switcher, back button.
    /// Supports Triangle, Cube, Physics (Pendulum), and Unit Circle lessons.
    /// </summary>
    public class ARLessonHUDController : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TMP_Text lessonTitleText;
        [SerializeField] private Button   backButton;

        [Header("Lesson Switcher")]
        [SerializeField] private Button btnSelectTriangle;
        [SerializeField] private Button btnSelectCube;
        [SerializeField] private Button btnSelectPhysics;
        [SerializeField] private Button btnSelectUnitCircle;

        [Header("Placement")]
        [SerializeField] private GameObject placementHintPanel;
        [SerializeField] private TMP_Text   hintText;
        [SerializeField] private Button     btnResetPlacement;

        [Header("Control Panels")]
        [SerializeField] private GameObject triangleControlPanel;
        [SerializeField] private GameObject physicsControlPanel;
        [SerializeField] private GameObject cubeControlPanel;

        [Header("Quiz Button")]
        [SerializeField] private Button btnStartQuiz;

        [Header("Toggle")]
        [SerializeField] private Button   btnToggleControls;
        [SerializeField] private TMP_Text btnToggleLabel;

        [Header("Dependencies")]
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private LessonManager      lessonManager;

        private bool _controlsVisible = false;
        private bool _hintHidden      = false;

        void Start()
        {
            backButton?.onClick.AddListener(() =>
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneMainMenu));

            btnSelectTriangle?.onClick.AddListener(()   => SwitchLesson(LessonType.Triangle));
            btnSelectCube?.onClick.AddListener(()       => SwitchLesson(LessonType.Cube));
            btnSelectPhysics?.onClick.AddListener(()    => SwitchLesson(LessonType.Physics));
            btnSelectUnitCircle?.onClick.AddListener(() => SwitchLesson(LessonType.UnitCircle));

            btnToggleControls?.onClick.AddListener(ToggleControls);
            btnResetPlacement?.onClick.AddListener(ResetPlacement);

            btnStartQuiz?.onClick.AddListener(() =>
            {
                QuizManager.SelectedLessonId = LessonManager.SelectedLesson switch
                {
                    LessonType.Triangle   => "triangle",
                    LessonType.Cube       => "cube",
                    LessonType.Physics    => "pendulum",
                    LessonType.UnitCircle => "unit_circle",
                    _                     => "triangle",
                };
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneQuiz);
            });

            if (placementManager != null)
                placementManager.OnObjectPlaced.AddListener(OnObjectPlaced);

            ARSession.stateChanged += OnARStateChanged;

            SetActivePanel(null);
            placementHintPanel?.SetActive(true);
            _hintHidden = false;
            if (hintText != null)
                hintText.text = "Scan a flat surface, then tap to place.";

            UpdateTitle();
            EnforceTextColors();
        }

        void Update()
        {
            // Polling fallback: if placement has happened but the hint is still showing,
            // hide it. Catches cases where the UnityEvent listener fired before Start().
            if (!_hintHidden && placementManager != null && placementManager.HasPlaced)
                OnObjectPlacedSimple();
        }

        private void OnARStateChanged(ARSessionStateChangedEventArgs args)
        {
            if ((args.state == ARSessionState.SessionTracking ||
                 args.state == ARSessionState.SessionInitializing) &&
                placementManager != null && placementManager.HasPlaced)
            {
                OnObjectPlacedSimple();
            }
        }

        private void SwitchLesson(LessonType type)
        {
            LessonManager.SelectedLesson = type;
            lessonManager?.LoadLesson(type);
            SetActivePanel(null);
            placementHintPanel?.SetActive(true);
            _hintHidden = false;
            _controlsVisible = false;
            UpdateTitle();
        }

        private void ResetPlacement()
        {
            placementManager?.ResetPlacement();
            SetActivePanel(null);
            placementHintPanel?.SetActive(true);
            _hintHidden = false;
            if (hintText != null)
                hintText.text = "Scan a flat surface, then tap to place.";
            _controlsVisible = false;
            if (btnToggleLabel != null) btnToggleLabel.text = "Show Controls";
        }

        private void OnObjectPlaced(GameObject obj, Pose pose) => OnObjectPlacedSimple();

        public void OnObjectPlacedSimple()
        {
            if (_hintHidden) return;
            _hintHidden = true;
            placementHintPanel?.SetActive(false);
            if (hintText != null)
                hintText.text = "Pinch to scale · Drag to move · 2-finger rotate";
            ShowControlsForCurrentLesson();
            _controlsVisible = true;
            if (btnToggleLabel != null) btnToggleLabel.text = "Hide Controls";
        }

        private void ToggleControls()
        {
            _controlsVisible = !_controlsVisible;
            if (_controlsVisible)
                ShowControlsForCurrentLesson();
            else
                SetActivePanel(null);

            if (btnToggleLabel != null)
                btnToggleLabel.text = _controlsVisible ? "Hide Controls" : "Show Controls";
        }

        private void ShowControlsForCurrentLesson()
        {
            SetActivePanel(LessonManager.SelectedLesson switch
            {
                LessonType.Triangle   => triangleControlPanel,
                LessonType.Cube       => cubeControlPanel,
                LessonType.Physics    => physicsControlPanel,
                LessonType.UnitCircle => null,
                _                     => null,
            });
        }

        private void SetActivePanel(GameObject active)
        {
            if (triangleControlPanel != null) triangleControlPanel.SetActive(active == triangleControlPanel);
            if (physicsControlPanel  != null) physicsControlPanel.SetActive(active  == physicsControlPanel);
            if (cubeControlPanel     != null) cubeControlPanel.SetActive(active     == cubeControlPanel);
        }

        private void UpdateTitle()
        {
            if (lessonTitleText == null) return;
            lessonTitleText.text = LessonManager.SelectedLesson switch
            {
                LessonType.Triangle   => "Triangle Lesson",
                LessonType.Cube       => "Cube Lesson",
                LessonType.Physics    => "Pendulum Lesson",
                LessonType.UnitCircle => "Unit Circle",
                _                     => "AR Lesson",
            };
        }

        /// <summary>
        /// Forces all TMP_Text in the scene to white where the color is invalid/magenta.
        /// Fixes a rendering issue on certain Android devices where the TMP material
        /// instance color is not applied correctly, causing text to appear magenta.
        /// </summary>
        private static void EnforceTextColors()
        {
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Color c = t.color;
                // Reset to white if the colour is magenta (r≈1, g≈0, b≈1)
                // OR if it is already nominally white but the internal material state is dirty.
                bool isMagenta = c.r > 0.8f && c.g < 0.2f && c.b > 0.8f;
                if (isMagenta || (c.r > 0.9f && c.g > 0.9f && c.b > 0.9f))
                {
                    t.color = Color.white;
                    // Force TMP to rebuild its material — clears any per-instance tint
                    t.SetAllDirty();
                }
            }
        }

        void OnDestroy()
        {
            ARSession.stateChanged -= OnARStateChanged;
            if (placementManager != null)
                placementManager.OnObjectPlaced.RemoveListener(OnObjectPlaced);
        }
    }
}
