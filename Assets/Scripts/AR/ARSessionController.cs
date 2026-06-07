using System.Collections;
using AREducation.Data;
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
    [DefaultExecutionOrder(-10000)]
    public class ARSessionController : MonoBehaviour
    {
        private const float PermissionTimeoutSeconds = 15f;
        private const float AvailabilityTimeoutSeconds = 20f;
        private const float InstallTimeoutSeconds = 90f;
        private const float SessionStartTimeoutSeconds = 18f;

        [SerializeField] private ARSession arSession;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARCameraManager cameraManager;
        [SerializeField] private ARCameraBackground cameraBackground;
        [SerializeField] private GameObject arNotSupportedPanel;
        [SerializeField] private GameObject arInitializingPanel;
        [SerializeField] private bool disableMatchFrameRateDuringStartup = true;

        public UnityEvent OnARReady;
        public UnityEvent OnARNotSupported;
        public UnityEvent OnCameraPermissionDenied;

        void Awake()
        {
            ResolveDependencies();
            SetARComponentsEnabled(false);
        }

        void OnEnable()  => ARSession.stateChanged += HandleARStateChanged;
        void OnDisable() => ARSession.stateChanged -= HandleARStateChanged;

        IEnumerator Start()
        {
            DiagnosticsLogger.Log($"Opening AR lesson. {DiagnosticsLogger.BuildDeviceSnapshot()}");
            arInitializingPanel?.SetActive(true);
            arNotSupportedPanel?.SetActive(false);

#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                DiagnosticsLogger.Log("Requesting Android camera permission.");
                Permission.RequestUserPermission(Permission.Camera);
                yield return WaitForCondition(
                    () => Permission.HasUserAuthorizedPermission(Permission.Camera),
                    PermissionTimeoutSeconds);

                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    FailStartup("Camera permission was not granted.", OnCameraPermissionDenied);
                    yield break;
                }
            }
#endif

            if (arSession == null)
            {
                FailStartup("ARSession is missing from the AR lesson scene.", OnARNotSupported);
                yield break;
            }

            yield return CheckAvailabilityWithTimeout();
            if (ARSession.state == ARSessionState.None ||
                ARSession.state == ARSessionState.CheckingAvailability)
            {
                FailStartup($"AR availability check timed out. State: {ARSession.state}", OnARNotSupported);
                yield break;
            }

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                DiagnosticsLogger.Log("ARCore is supported but needs install/update. Starting ARSession.Install.");
                yield return RunWithTimeout(ARSession.Install(), InstallTimeoutSeconds);
                DiagnosticsLogger.Log($"ARCore install/update finished with ARSession state: {ARSession.state}");
            }

            if (ARSession.state == ARSessionState.Unsupported ||
                ARSession.state == ARSessionState.NeedsInstall)
            {
                FailStartup($"AR is not available after setup. State: {ARSession.state}", OnARNotSupported);
                yield break;
            }

            if (disableMatchFrameRateDuringStartup)
                arSession.matchFrameRateRequested = false;

            SetARComponentsEnabled(true);
            DiagnosticsLogger.Log($"AR components enabled. State: {ARSession.state}");

            yield return WaitForCondition(
                () => ARSession.state == ARSessionState.SessionInitializing ||
                      ARSession.state == ARSessionState.SessionTracking,
                SessionStartTimeoutSeconds);

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                FailStartup($"AR session did not start. State: {ARSession.state}", OnARNotSupported);
                yield break;
            }

            arInitializingPanel?.SetActive(false);
            DiagnosticsLogger.Log($"AR lesson ready. State: {ARSession.state}");
            OnARReady?.Invoke();
        }

        private IEnumerator CheckAvailabilityWithTimeout()
        {
            if (ARSession.state == ARSessionState.None ||
                ARSession.state == ARSessionState.CheckingAvailability)
            {
                DiagnosticsLogger.Log("Checking AR availability.");
                yield return RunWithTimeout(ARSession.CheckAvailability(), AvailabilityTimeoutSeconds);
                DiagnosticsLogger.Log($"AR availability check finished with state: {ARSession.state}");
            }
        }

        private IEnumerator RunWithTimeout(IEnumerator operation, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (!operation.MoveNext())
                    yield break;

                yield return operation.Current;
            }

            DiagnosticsLogger.Log($"AR operation timed out after {timeoutSeconds:0}s. State: {ARSession.state}");
        }

        private IEnumerator WaitForCondition(System.Func<bool> condition, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
        }

        private void FailStartup(string message, UnityEvent callback)
        {
            DiagnosticsLogger.Log(message);
            SetARComponentsEnabled(false);
            arInitializingPanel?.SetActive(false);
            arNotSupportedPanel?.SetActive(true);
            callback?.Invoke();
        }

        private void ResolveDependencies()
        {
            if (arSession == null) arSession = FindFirstObjectByType<ARSession>();
            if (planeManager == null) planeManager = FindFirstObjectByType<ARPlaneManager>();
            if (raycastManager == null) raycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (cameraManager == null) cameraManager = FindFirstObjectByType<ARCameraManager>();
            if (cameraBackground == null) cameraBackground = FindFirstObjectByType<ARCameraBackground>();
        }

        private void SetARComponentsEnabled(bool enabled)
        {
            if (arSession != null) arSession.enabled = enabled;
            if (planeManager != null) planeManager.enabled = enabled;
            if (raycastManager != null) raycastManager.enabled = enabled;
            if (cameraManager != null) cameraManager.enabled = enabled;
            if (cameraBackground != null) cameraBackground.enabled = enabled;
        }

        private void HandleARStateChanged(ARSessionStateChangedEventArgs args)
        {
            DiagnosticsLogger.Log($"ARSession state changed: {args.state}");
            switch (args.state)
            {
                case ARSessionState.SessionInitializing:
                case ARSessionState.SessionTracking:
                    arInitializingPanel?.SetActive(false);
                    break;
            }
        }
    }
}
