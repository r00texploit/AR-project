using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace AREducation.Utils
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TMP_Text loadingText;

        // Scene build indices
        public const int SceneMainMenu        = 0;
        public const int SceneARLesson        = 1;
        public const int SceneQuiz            = 2;
        public const int SceneTeacherDashboard = 3;
        public const int SceneLogin           = 4;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingCanvas != null)
                loadingCanvas.gameObject.SetActive(false);
        }

        public void LoadScene(int buildIndex) =>
            StartCoroutine(LoadAsync(buildIndex));

        public void LoadScene(string sceneName) =>
            StartCoroutine(LoadAsync(
                SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity")));

        private IEnumerator LoadAsync(int buildIndex)
        {
            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(true);
                if (loadingText != null) loadingText.text = "Loading...";
                if (loadingBar  != null) loadingBar.value = 0f;
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] Scene {buildIndex} not found in build settings.");
                if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(false);
                yield break;
            }

            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (loadingBar != null) loadingBar.value = op.progress;
                yield return null;
            }

            if (loadingBar  != null) loadingBar.value = 1f;
            if (loadingText != null) loadingText.text = "Ready!";
            yield return new WaitForSeconds(0.15f);

            op.allowSceneActivation = true;

            // The loading canvas lives under this DontDestroyOnLoad object, so it
            // survives the scene swap — hide it once the new scene is active.
            while (!op.isDone)
                yield return null;

            if (loadingCanvas != null)
                loadingCanvas.gameObject.SetActive(false);
        }
    }
}
