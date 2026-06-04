using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Data;
using AREducation.Utils;

namespace AREducation.Teacher
{
    /// <summary>
    /// Loads quiz results from DataManager, populates a scrollable list,
    /// and supports filtering by lesson type.
    /// </summary>
    public class TeacherDashboardController : MonoBehaviour
    {
        [Header("Filter Buttons")]
        [SerializeField] private Button btnAll;
        [SerializeField] private Button btnTriangle;
        [SerializeField] private Button btnPhysics;
        [SerializeField] private Button btnCube;

        [Header("List")]
        [SerializeField] private Transform  listContent;    // ScrollView > Viewport > Content
        [SerializeField] private GameObject resultRowPrefab; // Prefab with ResultRowUI component

        [Header("Summary")]
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text averageText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        private List<QuizResult> _allResults = new List<QuizResult>();
        private string _currentFilter = "";

        void Start()
        {
            backButton?.onClick.AddListener(() =>
                SceneLoader.Instance?.LoadScene(SceneLoader.SceneMainMenu));

            btnAll?.onClick.AddListener(() => ApplyFilter(""));
            btnTriangle?.onClick.AddListener(() => ApplyFilter("triangle"));
            btnPhysics?.onClick.AddListener(()  => ApplyFilter("physics"));
            btnCube?.onClick.AddListener(()     => ApplyFilter("cube"));

            Refresh();
        }

        public void Refresh()
        {
            if (DataManager.Instance != null)
                _allResults = DataManager.Instance.GetAllResults();
            else
                _allResults = new List<QuizResult>();

            ApplyFilter(_currentFilter);
        }

        private void ApplyFilter(string lessonId)
        {
            _currentFilter = lessonId;

            List<QuizResult> filtered = string.IsNullOrEmpty(lessonId)
                ? _allResults
                : _allResults.Where(r =>
                    string.Equals(r.lessonId, lessonId, System.StringComparison.OrdinalIgnoreCase))
                  .ToList();

            PopulateList(filtered);

            // Summary
            float avg = filtered.Count > 0 ? filtered.Average(r => r.percentage) : 0f;
            if (summaryText != null)
                summaryText.text = $"{filtered.Count} result(s) shown";
            if (averageText != null)
                averageText.text = $"Average Score: {avg:F1}%";
        }

        private void PopulateList(List<QuizResult> results)
        {
            if (listContent == null) return;

            // Clear old rows
            foreach (Transform child in listContent)
                Destroy(child.gameObject);

            // Sort newest first
            var sorted = results.OrderByDescending(r => r.timestamp).ToList();

            foreach (var result in sorted)
            {
                if (resultRowPrefab == null)
                {
                    // Fallback: create a plain text row if no prefab assigned
                    CreateFallbackRow(result);
                    continue;
                }

                GameObject row = Instantiate(resultRowPrefab, listContent);
                ResultRowUI rowUI = row.GetComponent<ResultRowUI>();
                rowUI?.Setup(result);
            }
        }

        private void CreateFallbackRow(QuizResult result)
        {
            var go   = new GameObject("ResultRow");
            go.transform.SetParent(listContent, false);
            var text = go.AddComponent<TMP_Text>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 18;
            text.text     = $"{result.studentName}  |  " +
                            $"{result.lessonId}  |  " +
                            $"{result.score}/{result.totalQuestions}  |  " +
                            $"{result.timestamp[..10]}";
        }
    }
}
