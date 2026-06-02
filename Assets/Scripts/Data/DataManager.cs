using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AREducation.Data;

namespace AREducation.Data
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private const string ResultsKey = "quiz_results_v1";
        private const string StudentNameKey = "student_name";
        private const string AREnabledKey = "ar_enabled";

        private MockStudentData _mockData;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMockData();
        }

        private void LoadMockData()
        {
            TextAsset asset = Resources.Load<TextAsset>("StudentData/sample_students");
            if (asset != null)
                _mockData = JsonUtility.FromJson<MockStudentData>(asset.text);
            else
                _mockData = new MockStudentData
                {
                    students = Array.Empty<StudentProfile>(),
                    quizResults = Array.Empty<QuizResult>()
                };
        }

        public void SaveQuizResult(QuizResult result)
        {
            QuizResultList list = GetResultList();
            list.results.Add(result);
            PlayerPrefs.SetString(ResultsKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        public List<QuizResult> GetAllResults()
        {
            var real = GetResultList().results;
            var mock = _mockData?.quizResults != null
                ? new List<QuizResult>(_mockData.quizResults)
                : new List<QuizResult>();

            // Merge: mock first, real results on top
            var merged = new List<QuizResult>(mock);
            merged.AddRange(real);
            return merged;
        }

        public List<QuizResult> GetResultsForLesson(string lessonId)
        {
            return GetAllResults()
                .Where(r => string.Equals(r.lessonId, lessonId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<StudentProfile> GetStudents()
        {
            return _mockData?.students != null
                ? new List<StudentProfile>(_mockData.students)
                : new List<StudentProfile>();
        }

        public string GetStudentName() =>
            PlayerPrefs.GetString(StudentNameKey, "Student");

        public void SetStudentName(string name)
        {
            PlayerPrefs.SetString(StudentNameKey, name);
            PlayerPrefs.Save();
        }

        public bool IsAREnabled() =>
            PlayerPrefs.GetInt(AREnabledKey, 1) == 1;

        public void SetAREnabled(bool enabled)
        {
            PlayerPrefs.SetInt(AREnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ClearAllResults()
        {
            PlayerPrefs.DeleteKey(ResultsKey);
            PlayerPrefs.Save();
        }

        private QuizResultList GetResultList()
        {
            string json = PlayerPrefs.GetString(ResultsKey, "");
            if (string.IsNullOrEmpty(json))
                return new QuizResultList();
            try
            {
                return JsonUtility.FromJson<QuizResultList>(json) ?? new QuizResultList();
            }
            catch
            {
                return new QuizResultList();
            }
        }
    }
}
