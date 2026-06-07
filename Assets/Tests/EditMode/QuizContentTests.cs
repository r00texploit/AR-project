using AREducation.Data;
using NUnit.Framework;
using UnityEngine;

public class QuizContentTests
{
    [TestCase("triangle")]
    [TestCase("physics")]
    [TestCase("cube")]
    public void QuizJsonLoadsWithValidOptions(string lessonId)
    {
        TextAsset asset = Resources.Load<TextAsset>($"QuizData/{lessonId}_quiz");
        Assert.NotNull(asset, $"Missing quiz data for {lessonId}");

        QuizQuestionList list = JsonUtility.FromJson<QuizQuestionList>(asset.text);
        Assert.NotNull(list);
        Assert.NotNull(list.questions);
        Assert.Greater(list.questions.Length, 0);

        foreach (QuizQuestion question in list.questions)
        {
            Assert.AreEqual(lessonId, question.lessonId);
            Assert.IsNotEmpty(question.questionId);
            Assert.IsNotEmpty(question.questionText);
            Assert.NotNull(question.options);
            Assert.AreEqual(4, question.options.Length);
            Assert.GreaterOrEqual(question.correctIndex, 0);
            Assert.Less(question.correctIndex, question.options.Length);
            Assert.IsFalse(string.IsNullOrWhiteSpace(question.explanation));
        }
    }
}
