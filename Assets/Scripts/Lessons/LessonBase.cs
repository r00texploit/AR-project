using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AREducation.Lessons
{
    /// <summary>
    /// Abstract base class for all AR lesson objects.
    /// Subclasses implement OnInitialize() and OnReset().
    /// </summary>
    public abstract class LessonBase : MonoBehaviour
    {
        public string LessonId      { get; protected set; }
        public bool   IsInitialized { get; private set; }

        protected abstract void OnInitialize();
        protected abstract void OnReset();

        public void Initialize()
        {
            if (IsInitialized) return;
            OnInitialize();
            IsInitialized = true;
        }

        public void ResetLesson()
        {
            OnReset();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        // Called by ARPlacementManager after the object is placed in the world
        public virtual void OnPlaced(Pose worldPose) { }
    }
}
