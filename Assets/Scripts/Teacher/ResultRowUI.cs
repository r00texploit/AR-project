using UnityEngine;
using TMPro;
using AREducation.Data;

namespace AREducation.Teacher
{
    /// <summary>
    /// One row in the Teacher Dashboard result list.
    /// </summary>
    public class ResultRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lessonText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text gradeText;

        public void Setup(QuizResult result)
        {
            if (nameText   != null) nameText.text   = result.studentName;
            if (lessonText != null) lessonText.text = char.ToUpper(result.lessonId[0])
                                                     + result.lessonId[1..];
            if (scoreText  != null) scoreText.text  =
                $"{result.score}/{result.totalQuestions} ({result.percentage:F0}%)";
            if (dateText   != null) dateText.text   =
                result.timestamp.Length >= 10 ? result.timestamp[..10] : result.timestamp;
            if (gradeText  != null)
            {
                string grade = result.percentage >= 90f ? "A"
                             : result.percentage >= 80f ? "B"
                             : result.percentage >= 70f ? "C"
                             : result.percentage >= 60f ? "D" : "F";
                gradeText.text = grade;
            }
        }
    }
}
