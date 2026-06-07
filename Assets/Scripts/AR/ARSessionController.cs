using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Events;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace AREducation.AR
{
    /// <summary>
    /// Manages AR session lifecycle: permission requests, availability check, status feedback.
    /// </summary>
    public class ARSessionController : MonoBehaviour
    {
        [SerializeField] private ARSession arSession;
        [SerializeField] private GameObject arNotSupportedPanel;
        [SerializeField] private GameObject arInitializingPanel;

        public UnityEvent OnARReady;
        public UnityEvent OnARNotSupported;
        public UnityEvent OnCameraPermissionDenied;

        void OnEnable()  => ARSession.stateChanged += HandleARStateChanged;
        void OnDisable() => ARSession.stateChanged -= HandleARStateChanged;

        IEnumerator Start()
        {
            // Show initializing UI while checking
            arInitializingPanel?.SetActive(true);
            arNotSupportedPanel?.SetActive(false);

#if UNITY_ANDROID
            // Request camera permission before AR initialises
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                float deadline = Time.realtimeSinceStartup + 15f;
                yield return new WaitUntil(() =>
                    Permission.HasUserAuthorizedPermission(Permission.Camera) ||
                    Time.realtimeSinceStartup >= deadline);
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    arInitializingPanel?.SetActive(false);
                    arNotSupportedPanel?.SetActive(true);
                    OnCameraPermissionDenied?.Invoke();
                    yield break;
                }
            }
#endif

            // Wait for AR Foundation to finish its availability check
            if (ARSession.state == ARSessionState.None ||
                ARSession.state == ARSessionState.CheckingAvailability)
            {
                yield return ARSession.CheckAvailability();
            }

            // Check if AR is supported based on session state
            if (ARSession.state == ARSessionState.Unsupported ||
                ARSession.state == ARSessionState.NeedsInstall)
            {
                arInitializingPanel?.SetActive(false);
                arNotSupportedPanel?.SetActive(true);
                OnARNotSupported?.Invoke();
                yield break;
            }

            arInitializingPanel?.SetActive(false);
            OnARReady?.Invoke();
        }

        private void HandleARStateChanged(ARSessionStateChangedEventArgs args)
        {
            switch (args.state)
            {
                case ARSessionState.SessionInitializing:
                    arInitializingPanel?.SetActive(true);
                    break;
                case ARSessionState.SessionTracking:
                    arInitializingPanel?.SetActive(false);
                    break;
            }
        }
    }
}
