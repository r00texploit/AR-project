using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AREducation.Data
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private readonly LocalDataStore _store = new LocalDataStore();
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
            DiagnosticsLogger.Log("Data manager initialized.");
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
            _store.SaveResult(result);
        }

        public List<QuizResult> GetAllResults()
        {
            return _store.GetResults().results;
        }

        public List<QuizResult> GetResultsForLesson(string lessonId)
        {
            return GetAllResults()
                .Where(r => string.Equals(r.lessonId, lessonId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<StudentProfile> GetStudents()
        {
            return new List<StudentProfile> { GetStudentProfile() };
        }

        public StudentProfile GetStudentProfile() => _store.GetProfile();

        public void SaveStudentProfile(StudentProfile profile) => _store.SaveProfile(profile);

        public string GetStudentName() =>
            GetStudentProfile().studentName;

        public void SetStudentName(string name)
        {
            StudentProfile profile = GetStudentProfile();
            profile.studentName = string.IsNullOrWhiteSpace(name) ? "Student" : name.Trim();
            _store.SaveProfile(profile);
        }

        public void SetStudentDetails(string name, string gradeLevel, string className)
        {
            StudentProfile profile = GetStudentProfile();
            profile.studentName = string.IsNullOrWhiteSpace(name) ? "Student" : name.Trim();
            profile.gradeLevel = gradeLevel?.Trim() ?? "";
            profile.className = className?.Trim() ?? "";
            _store.SaveProfile(profile);
        }

        public bool IsAREnabled() =>
            _store.IsAREnabled();

        public void SetAREnabled(bool enabled) => _store.SetAREnabled(enabled);

        public void ClearAllResults()
        {
            _store.ClearResults();
        }

        public ReportExport ExportProgressReport()
        {
            ReportExport export = ReportGenerator.ExportPdf(GetStudentProfile(), GetAllResults());
            DiagnosticsLogger.Log($"Progress report exported: {export.filePath}");
            return export;
        }

        public bool ShareProgressReport()
        {
            ReportExport export = ExportProgressReport();
            return AndroidShareService.SharePdf(export.filePath, "Share AR Education report");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void LoadSampleResults()
        {
            _store.LoadSampleResults(_mockData?.quizResults);
        }
#endif
    }
}
