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

        [Header("Placement")]
        [SerializeField] private GameObject placementHintPanel;
        [SerializeField] private TMP_Text   hintText;

        [Header("Control Panels")]
        [SerializeField] private GameObject triangleControlPanel;
        [SerializeField] private GameObject physicsControlPanel;
        [SerializeField] private GameObject cubeControlPanel;

        [Header("Quiz Button")]
        [SerializeField] private Button btnStartQuiz;

        [Header("Toggle")]
        [SerializeField] private Button  btnToggleControls;
        [SerializeField] private TMP_Text btnToggleLabel;

        [Header("Dependencies")]
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private LessonManager      lessonManager;

        private bool _controlsVisible = false;

        void Start()
        {
            backButton?.onClick.AddListener(() =>
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneMainMenu));

            btnSelectTriangle?.onClick.AddListener(() => SwitchLesson(LessonType.Triangle));
            btnSelectCube?.onClick.AddListener(()     => SwitchLesson(LessonType.Cube));
            btnSelectPhysics?.onClick.AddListener(()  => SwitchLesson(LessonType.Physics));

            btnToggleControls?.onClick.AddListener(ToggleControls);

            btnStartQuiz?.onClick.AddListener(() =>
            {
                QuizManager.SelectedLessonId = LessonManager.SelectedLesson switch
                {
                    LessonType.Triangle => "triangle",
                    LessonType.Cube     => "cube",
                    LessonType.Physics  => "physics",
                    _                   => "triangle",
                };
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneQuiz);
            });

            if (placementManager != null)
                placementManager.OnObjectPlaced.AddListener(OnObjectPlaced);

            // Initial state
            SetActivePanel(null);
            placementHintPanel?.SetActive(true);
            if (hintText != null)
                hintText.text = "Scan a flat surface, then tap to place.";

            UpdateTitle();
        }

        private void SwitchLesson(LessonType type)
        {
            LessonManager.SelectedLesson = type;
            lessonManager?.LoadLesson(type);
            SetActivePanel(null);
            placementHintPanel?.SetActive(true);
            _controlsVisible = false;
            UpdateTitle();
        }

        private void OnObjectPlaced(GameObject obj, Pose pose)
        {
            OnObjectPlacedSimple();
        }

        public void OnObjectPlacedSimple()
        {
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
                LessonType.Triangle => triangleControlPanel,
                LessonType.Cube     => cubeControlPanel,
                LessonType.Physics  => physicsControlPanel,
                _                   => null,
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
                LessonType.Triangle => "Triangle Lesson",
                LessonType.Cube     => "Cube Lesson",
                LessonType.Physics  => "Physics Lesson",
                _                   => "AR Lesson",
            };
        }

        void OnDestroy()
        {
            if (placementManager != null)
                placementManager.OnObjectPlaced.RemoveListener(OnObjectPlaced);
        }
    }
}
