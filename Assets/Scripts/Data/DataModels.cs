using System;
using System.Collections.Generic;

namespace AREducation.Data
{
    [Serializable]
    public class StudentProfile
    {
        public string studentId;
        public string studentName;
        public string gradeLevel;
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
        public QuizQuestion[] questions;
    }

    [Serializable]
    public class QuizResult
    {
        public string studentId;
        public string studentName;
        public string lessonId;
        public string lessonTitle;
        public int score;
        public int totalQuestions;
        public float percentage;
        public string timestamp;  // ISO 8601
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
}
