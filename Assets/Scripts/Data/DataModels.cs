using System;
using System.Collections.Generic;

namespace AREducation.Data
{
    [Serializable]
    public class AIRecommendation
    {
        public string recommendationId;
        public string userId;
        public string[] weakTopics;
        public string[] suggestedLessons;
        public string date;
    }

    [Serializable]
    public class StudentProfile
    {
        public string studentId;
        public string studentName;
        public string gradeLevel;
        public string className;
        public string createdAt;
        // Firebase / XP fields (added for graduation project)
        public int    xp               = 0;
        public int    level            = 1;
        public string registrationDate = "";
    }

    [Serializable]
    public class StudentList
    {
        public List<StudentProfile> students;
    }

    [Serializable]
    public class QuizQuestion
    {
        public string questionId;
        public string lessonId;
        public string questionText;
        public string[] options;  // Always 4 elements
        public int correctIndex;  // 0-3
        public string explanation;
    }

    [Serializable]
    public class QuizQuestionList
    {
        public string lessonPassage;   // optional reading passage shown above questions
        public QuizQuestion[] questions;
    }

    [Serializable]
    public class QuizResult
    {
        public string attemptId;
        public string studentId;
        public string studentName;
        public string lessonId;
        public string lessonTitle;
        public int score;
        public int totalQuestions;
        public float percentage;
        public string timestamp;  // ISO 8601
        public float durationSeconds;
        public string appVersion;
    }

    // Wrapper needed because JsonUtility cannot deserialize root JSON arrays
    [Serializable]
    public class QuizResultList
    {
        public List<QuizResult> results = new List<QuizResult>();
    }

    [Serializable]
    public class MockStudentData
    {
        public StudentProfile[] students;
        public QuizResult[] quizResults;
    }

    [Serializable]
    public class LessonConfig
    {
        public string lessonId;
        public string displayName;
        public string description;
    }

    [Serializable]
    public class ReportExport
    {
        public string reportId;
        public string studentId;
        public string createdAt;
        public string filePath;
        public string[] includedLessonIds;
    }
}
