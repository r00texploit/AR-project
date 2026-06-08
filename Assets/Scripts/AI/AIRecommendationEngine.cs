using System;
using UnityEngine;
using AREducation.Data;

namespace AREducation.AI
{
    /// <summary>
    /// Rule-based AI recommendation engine.
    /// Analyses a quiz result and generates lesson suggestions for weak topics.
    /// </summary>
    public static class AIRecommendationEngine
    {
        private const float WeakThreshold = 0.6f; // below 60% → weak topic

        public static AIRecommendation Analyze(QuizResult result)
        {
            float pct = result.totalQuestions > 0
                ? (float)result.score / result.totalQuestions
                : 0f;

            bool isWeak = pct < WeakThreshold;

            string[] weakTopics       = isWeak ? new[] { result.lessonTitle ?? result.lessonId } : Array.Empty<string>();
            string[] suggestedLessons = isWeak ? new[] { result.lessonId } : Array.Empty<string>();

            DiagnosticsLogger.Log(
                $"[AI] {result.lessonId}: {pct:P0} — " +
                (isWeak ? $"Weak topic detected. Recommending re-study." : "Performance OK."));

            return new AIRecommendation
            {
                recommendationId = Guid.NewGuid().ToString("N"),
                userId           = result.studentId,
                weakTopics       = weakTopics,
                suggestedLessons = suggestedLessons,
                date             = DateTime.UtcNow.ToString("o"),
            };
        }

        public static string GetFeedbackMessage(QuizResult result)
        {
            float pct = result.totalQuestions > 0
                ? (float)result.score / result.totalQuestions
                : 0f;

            return pct switch
            {
                >= 0.9f => "Excellent! You've mastered this topic.",
                >= 0.7f => "Good work! Review the explanations to reinforce your understanding.",
                >= 0.6f => "Keep going! Practice a few more times to improve.",
                _       => "This topic needs more attention. Try the lesson again and retake the quiz.",
            };
        }

        public static int CalculateXP(QuizResult result)
        {
            float pct = result.totalQuestions > 0
                ? (float)result.score / result.totalQuestions
                : 0f;
            // Base XP: 50 for completing a quiz, up to +100 for a perfect score
            return 50 + Mathf.RoundToInt(pct * 100f);
        }

        public static int CalculateLevel(int totalXP)
        {
            // Level thresholds: 1=0, 2=300, 3=700, 4=1200, 5=1800, ...
            if (totalXP < 300)  return 1;
            if (totalXP < 700)  return 2;
            if (totalXP < 1200) return 3;
            if (totalXP < 1800) return 4;
            if (totalXP < 2500) return 5;
            return 6 + (totalXP - 2500) / 1000;
        }
    }
}
