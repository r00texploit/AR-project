using System;
using System.Collections.Generic;
using UnityEngine;
using AREducation.AI;
using AREducation.Data;

namespace AREducation.Quiz
{
    /// <summary>
    /// Loads quiz questions from Resources, tracks score, fires events for the UI.
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        // Set by MainMenuController before loading the Quiz scene
        public static string SelectedLessonId = "triangle";

        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int  _currentIndex;
        private int  _score;
        private float _startedAt;

        // Events consumed by QuizUIController
        public event Action<QuizQuestion, int, int>   OnQuestionChanged;   // question, idx, total
        public event Action<bool, string, int>         OnAnswerChecked;     // correct, explanation, currentScore
        public event Action<int, int>                  OnQuizComplete;      // finalScore, total
        public event Action<AIRecommendation, string>  OnRecommendationReady; // rec, feedbackMsg

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => LoadQuiz(SelectedLessonId);

        public void LoadQuiz(string lessonId)
        {
            _currentIndex = 0;
            _score        = 0;
            _startedAt    = Time.realtimeSinceStartup;

            TextAsset asset = Resources.Load<TextAsset>($"QuizData/{lessonId}_quiz");
            if (asset == null)
            {
                Debug.LogError($"[QuizManager] Quiz data not found: QuizData/{lessonId}_quiz");
                return;
            }

            QuizQuestionList data = JsonUtility.FromJson<QuizQuestionList>(asset.text);
            if (data?.questions == null || data.questions.Length == 0)
            {
                Debug.LogError($"[QuizManager] No questions parsed for {lessonId}");
                return;
            }

            _questions = new List<QuizQuestion>(data.questions);
            Shuffle();
            ShowCurrent();
        }

        public void CheckAnswer(int optionIndex)
        {
            if (_currentIndex >= _questions.Count) return;

            QuizQuestion q = _questions[_currentIndex];
            bool correct   = optionIndex == q.correctIndex;
            if (correct) _score++;

            OnAnswerChecked?.Invoke(correct, q.explanation, _score);
        }

        public void NextQuestion()
        {
            _currentIndex++;
            if (_currentIndex >= _questions.Count)
                FinishQuiz();
            else
                ShowCurrent();
        }

        private void ShowCurrent() =>
            OnQuestionChanged?.Invoke(_questions[_currentIndex], _currentIndex, _questions.Count);

        private void FinishQuiz()
        {
            QuizResult result = BuildResult();
            DataManager.Instance?.SaveQuizResult(result);
            OnQuizComplete?.Invoke(_score, _questions.Count);

            // AI recommendation
            AIRecommendation rec = AIRecommendationEngine.Analyze(result);
            DataManager.Instance?.SaveAIRecommendation(rec);
            string feedback = AIRecommendationEngine.GetFeedbackMessage(result);
            OnRecommendationReady?.Invoke(rec, feedback);
        }

        private QuizResult BuildResult()
        {
            string lessonTitle = SelectedLessonId.Length > 0
                ? char.ToUpper(SelectedLessonId[0]) + SelectedLessonId[1..].Replace('_', ' ') + " Lesson"
                : SelectedLessonId;

            return new QuizResult
            {
                attemptId       = Guid.NewGuid().ToString("N"),
                studentId       = DataManager.Instance?.GetStudentProfile().studentId ?? "",
                studentName     = DataManager.Instance?.GetStudentName() ?? "Student",
                lessonId        = SelectedLessonId,
                lessonTitle     = lessonTitle,
                score           = _score,
                totalQuestions  = _questions.Count,
                percentage      = _questions.Count > 0 ? (float)_score / _questions.Count * 100f : 0f,
                timestamp       = DateTime.UtcNow.ToString("o"),
                durationSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _startedAt),
                appVersion      = Application.version,
            };
        }

        private void Shuffle()
        {
            for (int i = _questions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_questions[i], _questions[j]) = (_questions[j], _questions[i]);
            }
        }
    }
}
