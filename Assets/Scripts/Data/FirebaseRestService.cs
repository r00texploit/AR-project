using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace AREducation.Data
{
    /// <summary>
    /// Firebase Auth + Firestore REST API: sign in/up, read/write student data.
    /// Falls back silently when FirebaseConfig is not configured.
    /// </summary>
    public class FirebaseRestService : MonoBehaviour
    {
        public static FirebaseRestService Instance { get; private set; }

        public string CurrentUserId { get; private set; }
        public string CurrentUserEmail { get; private set; }
        public string IdToken { get; private set; }

        public UnityEvent<bool, string> OnAuthResult; // success, message

        private FirebaseConfig _config;
        public bool IsAvailable => _config != null && _config.IsConfigured;
        public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken);

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

            LoadStoredAuth();
        }

        void LoadStoredAuth()
        {
            IdToken = PlayerPrefs.GetString("firebase_id_token", "");
            CurrentUserId = PlayerPrefs.GetString("firebase_user_id", "");
            CurrentUserEmail = PlayerPrefs.GetString("firebase_user_email", "");
        }

        void StoreAuth(string idToken, string uid, string email)
        {
            IdToken = idToken;
            CurrentUserId = uid;
            CurrentUserEmail = email;
            PlayerPrefs.SetString("firebase_id_token", idToken);
            PlayerPrefs.SetString("firebase_user_id", uid);
            PlayerPrefs.SetString("firebase_user_email", email);
            PlayerPrefs.Save();
        }

        public void SignUp(string email, string password)
        {
            if (!IsAvailable)
            {
                OnAuthResult?.Invoke(false, "Firebase not configured.");
                return;
            }
            StartCoroutine(SignUpCoroutine(email, password));
        }

        public void SignIn(string email, string password)
        {
            if (!IsAvailable)
            {
                OnAuthResult?.Invoke(false, "Firebase not configured.");
                return;
            }
            StartCoroutine(SignInCoroutine(email, password));
        }

        public void SignOut()
        {
            IdToken = "";
            CurrentUserId = "";
            CurrentUserEmail = "";
            PlayerPrefs.DeleteKey("firebase_id_token");
            PlayerPrefs.DeleteKey("firebase_user_id");
            PlayerPrefs.DeleteKey("firebase_user_email");
            PlayerPrefs.Save();
            OnAuthResult?.Invoke(false, "Signed out.");
        }

        private IEnumerator SignUpCoroutine(string email, string password)
        {
            string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_config.apiKey}";
            string json = $"{{\"email\":\"{EscapeJson(email)}\",\"password\":\"{EscapeJson(password)}\",\"returnSecureToken\":true}}";
            byte[] data = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest req = UnityWebRequest.Post(url, "application/json");
            req.uploadHandler = new UploadHandlerRaw(data);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                DiagnosticsLogger.Log($"[Firebase] SignUp error: {req.error}");
                OnAuthResult?.Invoke(false, ParseAuthError(req.downloadHandler.text));
                yield break;
            }

            string responseText = req.downloadHandler.text;
            if (ExtractAuthResponse(responseText, out string idToken, out string uid))
            {
                StoreAuth(idToken, uid, email);
                DiagnosticsLogger.Log($"[Firebase] Signed up: {email}");
                OnAuthResult?.Invoke(true, "Sign up successful.");
            }
            else
            {
                OnAuthResult?.Invoke(false, "Invalid response from Firebase.");
            }
        }

        private IEnumerator SignInCoroutine(string email, string password)
        {
            string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_config.apiKey}";
            string json = $"{{\"email\":\"{EscapeJson(email)}\",\"password\":\"{EscapeJson(password)}\",\"returnSecureToken\":true}}";
            byte[] data = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest req = UnityWebRequest.Post(url, "application/json");
            req.uploadHandler = new UploadHandlerRaw(data);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                DiagnosticsLogger.Log($"[Firebase] SignIn error: {req.error}");
                OnAuthResult?.Invoke(false, ParseAuthError(req.downloadHandler.text));
                yield break;
            }

            string responseText = req.downloadHandler.text;
            if (ExtractAuthResponse(responseText, out string idToken, out string uid))
            {
                StoreAuth(idToken, uid, email);
                DiagnosticsLogger.Log($"[Firebase] Signed in: {email}");
                OnAuthResult?.Invoke(true, "Sign in successful.");
            }
            else
            {
                OnAuthResult?.Invoke(false, "Invalid response from Firebase.");
            }
        }

        public void GetResults(System.Action<QuizResult[]> callback)
        {
            if (!IsAvailable) { callback?.Invoke(System.Array.Empty<QuizResult>()); return; }
            StartCoroutine(GetDocumentsCoroutine("Results", callback));
        }

        public void GetStudentProfile(string userId, System.Action<StudentProfile> callback)
        {
            if (!IsAvailable) { callback?.Invoke(null); return; }
            StartCoroutine(GetDocumentCoroutine("Users", userId, callback));
        }

        private IEnumerator GetDocumentsCoroutine(string collection, System.Action<QuizResult[]> callback)
        {
            string url = $"{_config.FirestoreUrl}/{collection}";
            using UnityWebRequest req = UnityWebRequest.Get(url);
            if (!string.IsNullOrEmpty(IdToken))
                req.SetRequestHeader("Authorization", $"Bearer {IdToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                DiagnosticsLogger.Log($"[Firebase] Get {collection} error: {req.error}");
                callback?.Invoke(System.Array.Empty<QuizResult>());
                yield break;
            }

            try
            {
                var results = ParseDocumentsResponse(req.downloadHandler.text);
                callback?.Invoke(results);
            }
            catch (System.Exception e)
            {
                DiagnosticsLogger.Log($"[Firebase] Parse error: {e.Message}");
                callback?.Invoke(System.Array.Empty<QuizResult>());
            }
        }

        private IEnumerator GetDocumentCoroutine(string collection, string docId, System.Action<StudentProfile> callback)
        {
            string url = $"{_config.FirestoreUrl}/{collection}/{Uri.EscapeDataString(docId)}";
            using UnityWebRequest req = UnityWebRequest.Get(url);
            if (!string.IsNullOrEmpty(IdToken))
                req.SetRequestHeader("Authorization", $"Bearer {IdToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                DiagnosticsLogger.Log($"[Firebase] Get {collection}/{docId} error: {req.error}");
                callback?.Invoke(null);
                yield break;
            }

            try
            {
                var profile = ParseDocumentResponse(req.downloadHandler.text);
                callback?.Invoke(profile);
            }
            catch (System.Exception e)
            {
                DiagnosticsLogger.Log($"[Firebase] Parse error: {e.Message}");
                callback?.Invoke(null);
            }
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

        // ── Firebase Auth response parsing ──
        private static bool ExtractAuthResponse(string json, out string idToken, out string uid)
        {
            idToken = "";
            uid = "";
            try
            {
                int tokenIdx = json.IndexOf("\"idToken\":\"");
                if (tokenIdx < 0) return false;
                int tokenStart = tokenIdx + 11;
                int tokenEnd = json.IndexOf("\"", tokenStart);
                idToken = json.Substring(tokenStart, tokenEnd - tokenStart);

                int uidIdx = json.IndexOf("\"localId\":\"");
                if (uidIdx < 0) return false;
                int uidStart = uidIdx + 11;
                int uidEnd = json.IndexOf("\"", uidStart);
                uid = json.Substring(uidStart, uidEnd - uidStart);

                return !string.IsNullOrEmpty(idToken) && !string.IsNullOrEmpty(uid);
            }
            catch { return false; }
        }

        private static string ParseAuthError(string json)
        {
            try
            {
                int msgIdx = json.IndexOf("\"message\":\"");
                if (msgIdx >= 0)
                {
                    int start = msgIdx + 11;
                    int end = json.IndexOf("\"", start);
                    return json.Substring(start, end - start);
                }
            }
            catch { }
            return "Authentication failed.";
        }

        private static QuizResult[] ParseDocumentsResponse(string json)
        {
            var results = new System.Collections.Generic.List<QuizResult>();
            try
            {
                int idx = 0;
                while ((idx = json.IndexOf("\"documents\":[", idx)) >= 0 ||
                       (idx = json.IndexOf("{\"name\":\"", idx)) >= 0)
                {
                    var result = new QuizResult();
                    // Simple field extraction (naive but works for our flat structure)
                    if (ExtractField(json, idx, "studentId", out string studentId))
                        result.studentId = studentId;
                    if (ExtractField(json, idx, "studentName", out string studentName))
                        result.studentName = studentName;
                    if (ExtractField(json, idx, "lessonId", out string lessonId))
                        result.lessonId = lessonId;
                    if (ExtractField(json, idx, "lessonTitle", out string lessonTitle))
                        result.lessonTitle = lessonTitle;
                    if (ExtractField(json, idx, "score", out string score) && int.TryParse(score, out int scoreVal))
                        result.score = scoreVal;
                    if (ExtractField(json, idx, "totalQuestions", out string total) && int.TryParse(total, out int totalVal))
                        result.totalQuestions = totalVal;
                    if (ExtractField(json, idx, "percentage", out string pct) && float.TryParse(pct, out float pctVal))
                        result.percentage = pctVal;
                    if (ExtractField(json, idx, "timestamp", out string ts))
                        result.timestamp = ts;

                    if (!string.IsNullOrEmpty(result.studentId))
                        results.Add(result);

                    idx += 10;
                    if (idx > json.Length - 10) break;
                }
            }
            catch { }
            return results.ToArray();
        }

        private static StudentProfile ParseDocumentResponse(string json)
        {
            var profile = new StudentProfile();
            try
            {
                if (ExtractField(json, 0, "studentId", out string sid))
                    profile.studentId = sid;
                if (ExtractField(json, 0, "studentName", out string name))
                    profile.studentName = name;
                if (ExtractField(json, 0, "gradeLevel", out string grade))
                    profile.gradeLevel = grade;
                if (ExtractField(json, 0, "xp", out string xp) && int.TryParse(xp, out int xpVal))
                    profile.xp = xpVal;
                if (ExtractField(json, 0, "level", out string lvl) && int.TryParse(lvl, out int lvlVal))
                    profile.level = lvlVal;
            }
            catch { }
            return profile;
        }

        private static bool ExtractField(string json, int startIdx, string fieldName, out string value)
        {
            value = "";
            try
            {
                string search = $"\"{fieldName}\":{{\"stringValue\":\"";
                int idx = json.IndexOf(search, startIdx);
                if (idx < 0)
                {
                    search = $"\"{fieldName}\":\"";
                    idx = json.IndexOf(search, startIdx);
                    if (idx < 0) return false;
                    int vStart = idx + search.Length;
                    int vEnd = json.IndexOf("\"", vStart);
                    value = json.Substring(vStart, vEnd - vStart);
                    return true;
                }
                int valueStart = idx + search.Length;
                int valueEnd = json.IndexOf("\"", valueStart);
                value = json.Substring(valueStart, valueEnd - valueStart);
                return true;
            }
            catch { return false; }
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
