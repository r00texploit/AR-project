using UnityEngine;

namespace AREducation.Data
{
    [CreateAssetMenu(fileName = "FirebaseConfig", menuName = "AR Education/Firebase Config")]
    public class FirebaseConfig : ScriptableObject
    {
        [Header("Firebase Project Settings")]
        [Tooltip("Web API Key from Firebase Console > Project Settings > General")]
        public string apiKey = "";

        [Tooltip("Project ID from Firebase Console (e.g. my-project-12345)")]
        public string projectId = "";

        public bool IsConfigured => !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(projectId);

        public string FirestoreUrl =>
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";
    }
}
