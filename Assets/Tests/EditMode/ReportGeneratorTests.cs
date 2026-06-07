using System.Collections.Generic;
using System.Text;
using AREducation.Data;
using NUnit.Framework;

public class ReportGeneratorTests
{
    [Test]
    public void PdfStartsWithPdfHeaderAndContainsStudentText()
    {
        byte[] pdf = ReportGenerator.CreatePdf(
            new StudentProfile
            {
                studentId = "student-1",
                studentName = "Test Student",
                gradeLevel = "Grade 7",
                className = "A"
            },
            new List<QuizResult>
            {
                new QuizResult
                {
                    lessonId = "cube",
                    lessonTitle = "Cube Lesson",
                    score = 5,
                    totalQuestions = 5,
                    percentage = 100f,
                    timestamp = "2026-06-07T12:00:00Z"
                }
            });

        string text = Encoding.ASCII.GetString(pdf);
        Assert.IsTrue(text.StartsWith("%PDF"));
        Assert.IsTrue(text.Contains("Test Student"));
        Assert.IsTrue(text.Contains("Cube Lesson"));
    }
}
