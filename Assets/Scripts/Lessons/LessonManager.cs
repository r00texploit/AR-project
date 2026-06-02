using UnityEngine;
using AREducation.AR;

namespace AREducation.Lessons
{
    public enum LessonType { Triangle, Cube, Physics }

    /// <summary>
    /// Central orchestrator for the AR lesson scene.
    /// Reads LessonType.SelectedLesson (set by MainMenu) and activates the correct lesson.
    /// </summary>
    public class LessonManager : MonoBehaviour
    {
        // Set by MainMenuController before scene load
        public static LessonType SelectedLesson = LessonType.Triangle;

        [Header("Lesson Objects (pre-created, initially inactive)")]
        [SerializeField] private TriangleLessonController triangleLesson;
        [SerializeField] private CubeLessonController     cubeLesson;
        [SerializeField] private PhysicsLessonController  physicsLesson;

        [Header("AR Components")]
        [SerializeField] private ARPlacementManager placementManager;

        private LessonBase _currentLesson;

        void Start()
        {
            HideAll();
            LoadLesson(SelectedLesson);
        }

        public void LoadLesson(LessonType type)
        {
            HideAll();
            _currentLesson = type switch
            {
                LessonType.Triangle => triangleLesson,
                LessonType.Cube     => cubeLesson,
                LessonType.Physics  => physicsLesson,
                _                   => triangleLesson,
            };

            if (_currentLesson == null)
            {
                Debug.LogError($"[LessonManager] Lesson controller not assigned for {type}");
                return;
            }

            _currentLesson.Initialize();

            // Hand the lesson object to the placement manager
            if (placementManager != null)
                placementManager.SetPendingObject(_currentLesson.gameObject);
        }

        private void HideAll()
        {
            triangleLesson?.Hide();
            cubeLesson?.Hide();
            physicsLesson?.Hide();
        }

        public LessonBase GetCurrentLesson() => _currentLesson;
    }
}
