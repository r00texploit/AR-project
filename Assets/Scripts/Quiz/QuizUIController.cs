using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Data;
using AREducation.Utils;

namespace AREducation.Quiz
{
    /// <summary>
    /// Drives quiz UI: questions, option buttons, feedback, result screen.
    /// Subscribes to QuizManager events.
    /// </summary>
    public class QuizUIController : MonoBehaviour
    {
        [Header("Lesson Selection")]
        [SerializeField] private GameObject lessonSelectPanel;
        [SerializeField] private Button     btnTriangleQuiz;
        [SerializeField] private Button     btnPhysicsQuiz;
        [SerializeField] private Button     btnCubeQuiz;

        [Header("Quiz Panel")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TMP_Text   progressText;
        [SerializeField] private TMP_Text   questionText;
        [SerializeField] private Button[]   optionButtons;   // 4 buttons
        [SerializeField] private TMP_Text[] optionTexts;     // labels on each button

        [Header("Feedback")]
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private Button   nextButton;

        [Header("Results")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text   scoreText;
        [SerializeField] private TMP_Text   ratingText;
        [SerializeField] private Button     retryButton;
        [SerializeField] private Button     menuButton;
        [SerializeField] private Button     progressButton;
        [SerializeField] private Button     exportButton;
        [SerializeField] private TMP_Text   resultStatusText;

        [Header("Colors")]
        [SerializeField] private Color correctColor   = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color defaultColor   = Color.white;

        [Header("Passage (optional — auto-created if not wired in Inspector)")]
        [SerializeField] private GameObject passagePanel;
        [SerializeField] private TMP_Text   passageText;

        [Header("Back")]
        [SerializeField] private Button backButton;

        void Start()
        {
            // Subscribe to manager events
            QuizManager.Instance.OnQuestionChanged += DisplayQuestion;
            QuizManager.Instance.OnAnswerChecked   += ShowFeedback;
            QuizManager.Instance.OnQuizComplete    += ShowResults;

            // Wire option buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int idx = i; // closure capture
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(idx));
            }

            nextButton?.onClick.AddListener(() =>
            {
                SetFeedbackVisible(false);
                QuizManager.Instance.NextQuestion();
            });

            retryButton?.onClick.AddListener(() =>
            {
                resultPanel?.SetActive(false);
                quizPanel?.SetActive(true);
                QuizManager.Instance.LoadQuiz(QuizManager.SelectedLessonId);
                RefreshPassage();
            });

            menuButton?.onClick.AddListener(() =>
            {
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(SceneLoader.SceneMainMenu);
            });
            progressButton?.onClick.AddListener(() =>
            {
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(SceneLoader.SceneTeacherDashboard);
            });
            exportButton?.onClick.AddListener(ExportReport);

            backButton?.onClick.AddListener(() =>
            {
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(SceneLoader.SceneMainMenu);
            });

            // Lesson select panel buttons
            btnTriangleQuiz?.onClick.AddListener(() => StartQuizFor("triangle"));
            btnPhysicsQuiz?.onClick.AddListener(()  => StartQuizFor("physics"));
            btnCubeQuiz?.onClick.AddListener(()     => StartQuizFor("cube"));

            // Initial state
            quizPanel?.SetActive(false);
            resultPanel?.SetActive(false);
            SetFeedbackVisible(false);

            // If a lesson was pre-selected (from main menu), skip selector
            if (!string.IsNullOrEmpty(QuizManager.SelectedLessonId))
            {
                lessonSelectPanel?.SetActive(false);
                quizPanel?.SetActive(true);
                RefreshPassage(); // QuizManager.Start() already called LoadQuiz
            }
            else
            {
                lessonSelectPanel?.SetActive(true);
            }
        }

        private void StartQuizFor(string lessonId)
        {
            QuizManager.SelectedLessonId = lessonId;
            lessonSelectPanel?.SetActive(false);
            quizPanel?.SetActive(true);
            QuizManager.Instance.LoadQuiz(lessonId);
            RefreshPassage();
        }

        private void DisplayQuestion(QuizQuestion q, int index, int total)
        {
            if (progressText != null)
                progressText.text = $"Q {index + 1} / {total}";

            if (questionText != null)
                questionText.text = q.questionText;

            for (int i = 0; i < optionButtons.Length && i < q.options.Length; i++)
            {
                if (optionTexts != null && i < optionTexts.Length)
                    optionTexts[i].text = q.options[i];

                optionButtons[i].interactable = true;
                SetButtonColor(optionButtons[i], defaultColor);
            }

            SetFeedbackVisible(false);
        }

        private void OnOptionSelected(int index)
        {
            foreach (var btn in optionButtons)
                btn.interactable = false;

            QuizManager.Instance.CheckAnswer(index);
        }

        private void ShowFeedback(bool correct, string explanation, int currentScore)
        {
            if (feedbackText != null)
            {
                feedbackText.text  = correct ? "Correct!" : "Incorrect";
                feedbackText.color = correct ? correctColor : incorrectColor;
            }

            if (explanationText != null)
                explanationText.text = explanation;

            SetFeedbackVisible(true);
        }

        private void ShowResults(int score, int total)
        {
            quizPanel?.SetActive(false);
            if (passagePanel != null) passagePanel.SetActive(false);
            resultPanel?.SetActive(true);

            if (scoreText != null)
                scoreText.text = $"You scored {score} / {total}!";

            if (ratingText != null)
            {
                float pct = total > 0 ? (float)score / total * 100f : 0f;
                ratingText.text = pct >= 80f ? "Excellent! 🌟"
                                : pct >= 60f ? "Good job! 👍"
                                :              "Keep practising! 📚";
            }

            if (resultStatusText != null)
                resultStatusText.text = "Your result was saved locally on this device.";
        }

        private void ExportReport()
        {
            if (DataManager.Instance == null)
            {
                if (resultStatusText != null) resultStatusText.text = "Progress data is not ready yet.";
                return;
            }

            bool shared = DataManager.Instance.ShareProgressReport();
            if (resultStatusText != null)
                resultStatusText.text = shared
                    ? "Progress report exported. Choose an app to share it."
                    : "Report export failed. Try again.";
        }

        private void RefreshPassage()
        {
            string passage = QuizManager.Instance?.CurrentPassage ?? "";
            bool hasPassage = !string.IsNullOrWhiteSpace(passage);

            if (!hasPassage)
            {
                if (passagePanel != null) passagePanel.SetActive(false);
                return;
            }

            EnsurePassagePanel();
            passagePanel.SetActive(true);
            if (passageText != null) passageText.text = passage;
        }

        private void EnsurePassagePanel()
        {
            if (passagePanel != null) return;
            if (quizPanel == null) return;

            var go = new GameObject("PassagePanel");
            go.transform.SetParent(quizPanel.transform, false);
            go.transform.SetAsFirstSibling();

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.12f, 0.22f, 0.97f);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.02f, 0.60f);
            rt.anchorMax = new Vector2(0.98f, 0.97f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var textGO = new GameObject("PassageText");
            textGO.transform.SetParent(go.transform, false);

            var textRT = (RectTransform)textGO.transform;
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(18, 12);
            textRT.offsetMax = new Vector2(-18, -12);

            passageText = textGO.AddComponent<TextMeshProUGUI>();
            passageText.fontSize = 22;
            passageText.color = new Color(0.92f, 0.96f, 1f);
            passageText.alignment = TextAlignmentOptions.TopLeft;
            passageText.overflowMode = TextOverflowModes.Ellipsis;

            passagePanel = go;
        }

        private void SetFeedbackVisible(bool show)
        {
            if (feedbackText     != null) feedbackText.gameObject.SetActive(show);
            if (explanationText  != null) explanationText.gameObject.SetActive(show);
            if (nextButton       != null) nextButton.gameObject.SetActive(show);
        }

        private static void SetButtonColor(Button btn, Color color)
        {
            ColorBlock cb   = btn.colors;
            cb.normalColor  = color;
            btn.colors      = cb;
        }

        void OnDestroy()
        {
            if (QuizManager.Instance == null) return;
            QuizManager.Instance.OnQuestionChanged -= DisplayQuestion;
            QuizManager.Instance.OnAnswerChecked   -= ShowFeedback;
            QuizManager.Instance.OnQuizComplete    -= ShowResults;
        }
    }
}
