using AREducation.Data;
using NUnit.Framework;
using UnityEngine;

public class LocalDataStoreTests
{
    [SetUp]
    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void CreatesStableProfileWhenEmpty()
    {
        var store = new LocalDataStore();
        StudentProfile profile = store.GetProfile();

        Assert.IsFalse(string.IsNullOrWhiteSpace(profile.studentId));
        Assert.AreEqual("Student", profile.studentName);
        Assert.AreEqual(profile.studentId, store.GetProfile().studentId);
    }

    [Test]
    public void CorruptResultsJsonFallsBackToEmptyList()
    {
        PlayerPrefs.SetString("quiz_results_v2", "{not valid json");
        var store = new LocalDataStore();

        Assert.AreEqual(0, store.GetResults().results.Count);
    }

    [Test]
    public void SaveResultAddsProfileAndAttemptMetadata()
    {
        var store = new LocalDataStore();
        store.SaveResult(new QuizResult
        {
            lessonId = "triangle",
            lessonTitle = "Triangle Lesson",
            score = 4,
            totalQuestions = 5,
            percentage = 80f
        });

        QuizResult saved = store.GetResults().results[0];
        Assert.IsFalse(string.IsNullOrWhiteSpace(saved.attemptId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(saved.studentId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(saved.timestamp));
        Assert.IsFalse(string.IsNullOrWhiteSpace(saved.appVersion));
    }
}
