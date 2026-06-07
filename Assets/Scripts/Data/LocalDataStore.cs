using System;
using System.Collections.Generic;
using UnityEngine;

namespace AREducation.Data
{
    /// <summary>
    /// Local-first persistence wrapper. Keeps production data on-device and
    /// guards callers from corrupt PlayerPrefs JSON.
    /// </summary>
    public class LocalDataStore
    {
        private const string ResultsKey = "quiz_results_v2";
        private const string LegacyResultsKey = "quiz_results_v1";
        private const string ProfileKey = "student_profile_v1";
        private const string AREnabledKey = "ar_enabled";

        public StudentProfile GetProfile()
        {
            string json = PlayerPrefs.GetString(ProfileKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    StudentProfile parsed = JsonUtility.FromJson<StudentProfile>(json);
                    if (IsValidProfile(parsed))
                        return parsed;
                }
                catch (Exception ex)
                {
                    DiagnosticsLogger.Log($"Profile JSON was invalid: {ex.Message}");
                }
            }

            StudentProfile profile = new StudentProfile
            {
                studentId = Guid.NewGuid().ToString("N"),
                studentName = PlayerPrefs.GetString("student_name", "Student"),
                gradeLevel = "",
                className = "",
                createdAt = DateTime.UtcNow.ToString("o")
            };
            SaveProfile(profile);
            return profile;
        }

        public void SaveProfile(StudentProfile profile)
        {
            if (profile == null)
                profile = new StudentProfile();

            if (string.IsNullOrWhiteSpace(profile.studentId))
                profile.studentId = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(profile.studentName))
                profile.studentName = "Student";
            if (string.IsNullOrWhiteSpace(profile.createdAt))
                profile.createdAt = DateTime.UtcNow.ToString("o");

            PlayerPrefs.SetString(ProfileKey, JsonUtility.ToJson(profile));
            PlayerPrefs.Save();
        }

        public QuizResultList GetResults()
        {
            QuizResultList current = ReadResultList(ResultsKey);
            if (current.results.Count > 0)
                return current;

            QuizResultList legacy = ReadResultList(LegacyResultsKey);
            if (legacy.results.Count > 0)
            {
                PlayerPrefs.SetString(ResultsKey, JsonUtility.ToJson(legacy));
                PlayerPrefs.Save();
            }
            return legacy;
        }

        public void SaveResult(QuizResult result)
        {
            if (result == null) return;

            StudentProfile profile = GetProfile();
            if (string.IsNullOrWhiteSpace(result.attemptId))
                result.attemptId = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(result.studentId))
                result.studentId = profile.studentId;
            if (string.IsNullOrWhiteSpace(result.studentName))
                result.studentName = profile.studentName;
            if (string.IsNullOrWhiteSpace(result.timestamp))
                result.timestamp = DateTime.UtcNow.ToString("o");
            if (string.IsNullOrWhiteSpace(result.appVersion))
                result.appVersion = Application.version;

            QuizResultList list = GetResults();
            list.results.Add(result);
            PlayerPrefs.SetString(ResultsKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        public void ClearResults()
        {
            PlayerPrefs.DeleteKey(ResultsKey);
            PlayerPrefs.DeleteKey(LegacyResultsKey);
            PlayerPrefs.Save();
        }

        public bool IsAREnabled() => PlayerPrefs.GetInt(AREnabledKey, 1) == 1;

        public void SetAREnabled(bool enabled)
        {
            PlayerPrefs.SetInt(AREnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void LoadSampleResults(QuizResult[] sampleResults)
        {
            if (sampleResults == null) return;
            QuizResultList list = GetResults();
            list.results.AddRange(sampleResults);
            PlayerPrefs.SetString(ResultsKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }
#endif

        private static bool IsValidProfile(StudentProfile profile)
        {
            return profile != null && !string.IsNullOrWhiteSpace(profile.studentId);
        }

        private static QuizResultList ReadResultList(string key)
        {
            string json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json))
                return new QuizResultList();

            try
            {
                QuizResultList list = JsonUtility.FromJson<QuizResultList>(json);
                if (list == null)
                    return new QuizResultList();
                if (list.results == null)
                    list.results = new List<QuizResult>();
                return list;
            }
            catch (Exception ex)
            {
                DiagnosticsLogger.Log($"Result JSON for {key} was invalid: {ex.Message}");
                return new QuizResultList();
            }
        }
    }
}
