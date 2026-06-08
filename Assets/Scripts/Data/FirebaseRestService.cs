using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AREducation.Data
{
    /// <summary>
    /// Sends data to Firebase Firestore via the REST API.
    /// Falls back silently when FirebaseConfig is not configured.
    /// </summary>
    public class FirebaseRestService : MonoBehaviour
    {
        public static FirebaseRestService Instance { get; private set; }

        private FirebaseConfig _config;
        public bool IsAvailable => _config != null && _config.IsConfigured;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _config = Resources.Load<FirebaseConfig>("FirebaseConfig");
            if (!IsAvailable)
                DiagnosticsLogger.Log("[Firebase] Config not found — using local storage only.");
            else
                DiagnosticsLogger.Log($"[Firebase] Ready. Project: {_config.projectId}");
        }

        public void SaveQuizResult(QuizResult r) =>
            StartCoroutine(PutDocument("Results", r.attemptId, BuildQuizResultJson(r), "SaveQuizResult"));

        public void SaveRecommendation(AIRecommendation rec) =>
            StartCoroutine(PutDocument("AI_Recommendations", rec.recommendationId, BuildRecommendationJson(rec), "SaveRecommendation"));

        public void UpsertStudentProfile(StudentProfile p) =>
            StartCoroutine(PutDocument("Users", string.IsNullOrEmpty(p.studentId) ? p.studentName : p.studentId, BuildStudentJson(p), "UpsertStudent"));

        private IEnumerator PutDocument(string collection, string docId, string json, string label)
        {
            if (!IsAvailable) yield break;

            string url = $"{_config.FirestoreUrl}/{collection}/{Uri.EscapeDataString(docId)}";
            byte[] data = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest req = UnityWebRequest.Put(url, data);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                DiagnosticsLogger.Log($"[Firebase] {label} error: {req.error} ({req.responseCode})");
            else
                DiagnosticsLogger.Log($"[Firebase] {label} saved.");
        }

        // ── JSON builders (manual — avoids Newtonsoft dependency for Firestore format) ──

        private static string BuildQuizResultJson(QuizResult r) => BuildDoc(
            S("attemptId",      r.attemptId),
            S("studentId",      r.studentId),
            S("studentName",    r.studentName),
            S("lessonId",       r.lessonId),
            S("lessonTitle",    r.lessonTitle),
            S("score",          r.score.ToString()),
            S("totalQuestions", r.totalQuestions.ToString()),
            S("percentage",     r.percentage.ToString("F2")),
            S("timestamp",      r.timestamp));

        private static string BuildRecommendationJson(AIRecommendation rec) => BuildDoc(
            S("recommendationId",  rec.recommendationId),
            S("userId",            rec.userId),
            S("weakTopics",        string.Join("|", rec.weakTopics ?? Array.Empty<string>())),
            S("suggestedLessons",  string.Join("|", rec.suggestedLessons ?? Array.Empty<string>())),
            S("date",              rec.date));

        private static string BuildStudentJson(StudentProfile p) => BuildDoc(
            S("studentId",        p.studentId),
            S("studentName",      p.studentName),
            S("gradeLevel",       p.gradeLevel ?? ""),
            S("xp",               p.xp.ToString()),
            S("level",            p.level.ToString()),
            S("registrationDate", p.registrationDate ?? ""));

        private static (string key, string val) S(string k, string v) => (k, v);

        private static string BuildDoc(params (string key, string val)[] fields)
        {
            var sb = new StringBuilder("{\"fields\":{");
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) sb.Append(',');
                string esc = fields[i].val.Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append($"\"{fields[i].key}\":{{\"stringValue\":\"{esc}\"}}");
            }
            sb.Append("}}");
            return sb.ToString();
        }
    }
}
