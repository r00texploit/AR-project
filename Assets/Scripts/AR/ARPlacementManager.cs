using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AREducation.AR
{
    /// <summary>
    /// Handles AR plane detection and tap-to-place for lesson objects.
    /// Fires OnObjectPlaced(GameObject, Pose) when placement succeeds.
    /// </summary>
    public class ARPlacementManager : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager  raycastManager;
        [SerializeField] private ARPlaneManager    planeManager;
        [SerializeField] private GameObject        placementIndicator; // optional visual
        [SerializeField] private bool              replaceable = false; // allow re-tapping to move

        public UnityEvent<GameObject, Pose> OnObjectPlaced;

        private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
        private GameObject _pendingObject;
        private bool _placementEnabled;
        private bool _hasPlaced;

        public void SetPendingObject(GameObject obj)
        {
            _pendingObject = obj;
            _hasPlaced = false;
            _placementEnabled = true;
            ShowIndicator(false);
        }

        public void EnablePlacement(bool enable) => _placementEnabled = enable;

        void Update()
        {
            if (!_placementEnabled) return;
            if (_hasPlaced && !replaceable) return;
            if (Input.touchCount != 1) return;

            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (IsPointerOverUI(touch.position)) return;

            if (!raycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
                return;

            Pose hitPose = Hits[0].pose;
            PlaceObject(hitPose);
        }

        private void PlaceObject(Pose pose)
        {
            if (_pendingObject == null) return;

            _pendingObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _pendingObject.SetActive(true);

            if (!_hasPlaced)
            {
                _hasPlaced = true;
                HidePlaneVisuals();
                OnObjectPlaced?.Invoke(_pendingObject, pose);
            }
        }

        private void HidePlaneVisuals()
        {
            if (planeManager == null) return;
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(false);
            planeManager.enabled = false;
        }

        private void ShowIndicator(bool show)
        {
            if (placementIndicator != null)
                placementIndicator.SetActive(show);
        }

        private static bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject(
                Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1);
        }
    }
}
