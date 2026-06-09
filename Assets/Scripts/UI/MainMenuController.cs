using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Data;
using AREducation.Lessons;
using AREducation.Quiz;
using AREducation.Utils;

namespace AREducation.UI
{
    /// <summary>
    /// Controls the Main Menu scene: navigates to AR lesson, quiz, teacher, and settings.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button btnStartAR;
        [SerializeField] private Button btnQuizzes;
        [SerializeField] private Button btnTeacher;
        [SerializeField] private Button btnSettings;

        [Header("Welcome")]
        [SerializeField] private TMP_Text welcomeText;

        [Header("AR Lesson Selection Panel")]
        [SerializeField] private GameObject lessonSelectPanel;
        [SerializeField] private Button     btnLessonTriangle;
        [SerializeField] private Button     btnLessonCube;
        [SerializeField] private Button     btnLessonPhysics;
        [SerializeField] private Button     btnCloseLessonPanel;

        [Header("Quiz Selection Panel")]
        [SerializeField] private GameObject quizSelectPanel;
        [SerializeField] private Button     btnQuizTriangle;
        [SerializeField] private Button     btnQuizPhysics;
        [SerializeField] private Button     btnQuizCube;
        [SerializeField] private Button     btnCloseQuizPanel;

        [Header("Settings Panel")]
        [SerializeField] private GameObject   settingsPanel;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField gradeInput;
        [SerializeField] private TMP_InputField classInput;
        [SerializeField] private Toggle        arToggle;
        [SerializeField] private Button        btnSaveSettings;
        [SerializeField] private Button        btnCloseSettings;
        [SerializeField] private Button        btnSignOut;

        void Start()
        {
            // Check auth: if Firebase is available and user not authenticated, redirect to login
            if (FirebaseRestService.Instance != null && FirebaseRestService.Instance.IsAvailable &&
                !FirebaseRestService.Instance.IsAuthenticated)
            {
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneLogin);
                return;
            }

            // Main menu
            btnStartAR?.onClick.AddListener(() =>
            {
                lessonSelectPanel?.SetActive(true);
                quizSelectPanel?.SetActive(false);
            });
            btnQuizzes?.onClick.AddListener(() =>
            {
                quizSelectPanel?.SetActive(true);
                lessonSelectPanel?.SetActive(false);
            });
            btnTeacher?.onClick.AddListener(() =>
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneTeacherDashboard));
            btnSettings?.onClick.AddListener(OpenSettings);

            // AR lesson selection
            btnLessonTriangle?.onClick.AddListener(() => LaunchARLesson(LessonType.Triangle));
            btnLessonCube?.onClick.AddListener(()     => LaunchARLesson(LessonType.Cube));
            btnLessonPhysics?.onClick.AddListener(()  => LaunchARLesson(LessonType.Physics));
            btnCloseLessonPanel?.onClick.AddListener(() => lessonSelectPanel?.SetActive(false));

            // Quiz selection
            btnQuizTriangle?.onClick.AddListener(() => LaunchQuiz("triangle"));
            btnQuizPhysics?.onClick.AddListener(()  => LaunchQuiz("physics"));
            btnQuizCube?.onClick.AddListener(()     => LaunchQuiz("cube"));
            btnCloseQuizPanel?.onClick.AddListener(() => quizSelectPanel?.SetActive(false));

            // Settings
            btnSaveSettings?.onClick.AddListener(SaveSettings);
            btnCloseSettings?.onClick.AddListener(() => settingsPanel?.SetActive(false));
            btnSignOut?.onClick.AddListener(() =>
            {
                FirebaseRestService.Instance?.SignOut();
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneLogin);
            });

            // Initial visibility
            lessonSelectPanel?.SetActive(false);
            quizSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);

            RefreshWelcome();
        }

        private void LaunchARLesson(LessonType type)
        {
            LessonManager.SelectedLesson = type;
            SceneLoader.Instance?.LoadScene(SceneLoader.SceneARLesson);
        }

        private void LaunchQuiz(string lessonId)
        {
            QuizManager.SelectedLessonId = lessonId;
            SceneLoader.Instance?.LoadScene(SceneLoader.SceneQuiz);
        }

        private void OpenSettings()
        {
            settingsPanel?.SetActive(true);
            if (nameInput != null && DataManager.Instance != null)
            {
                var profile = DataManager.Instance.GetStudentProfile();
                nameInput.text = profile.studentName;
                if (gradeInput != null) gradeInput.text = profile.gradeLevel;
                if (classInput != null) classInput.text = profile.className;
            }
            if (arToggle   != null && DataManager.Instance != null)
                arToggle.isOn = DataManager.Instance.IsAREnabled();
        }

        private void SaveSettings()
        {
            if (DataManager.Instance != null)
            {
                if (nameInput != null)
                    DataManager.Instance.SetStudentDetails(
                        nameInput.text,
                        gradeInput != null ? gradeInput.text : "",
                        classInput != null ? classInput.text : "");
                if (arToggle != null)
                    DataManager.Instance.SetAREnabled(arToggle.isOn);
            }
            settingsPanel?.SetActive(false);
            RefreshWelcome();
        }

        private void RefreshWelcome()
        {
            if (welcomeText == null) return;
            string name = DataManager.Instance?.GetStudentName() ?? "Student";
            welcomeText.text = $"Welcome, {name}!";
        }
    }
}
