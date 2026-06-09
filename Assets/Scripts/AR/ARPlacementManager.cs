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

        public bool HasPlaced => _hasPlaced;

        public void SetPendingObject(GameObject obj)
        {
            _pendingObject = obj;
            _hasPlaced = false;
            _placementEnabled = true;
            ShowIndicator(false);
        }

        public void EnablePlacement(bool enable) => _placementEnabled = enable;

        public void ResetPlacement()
        {
            _hasPlaced = false;
            _placementEnabled = true;
            if (_pendingObject != null)
                _pendingObject.SetActive(false);

            if (planeManager != null)
            {
                planeManager.enabled = true;
                foreach (var plane in planeManager.trackables)
                    plane.gameObject.SetActive(true);
            }
            ShowIndicator(false);
        }

        void Update()
        {
            if (!_placementEnabled) return;
            if (_hasPlaced && !replaceable) return;

            // Update placement indicator using screen-centre raycast
            UpdateIndicator();

            if (Input.touchCount != 1) return;

            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (IsPointerOverUI(touch.position)) return;

            if (raycastManager != null && raycastManager.enabled &&
                raycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
            {
                PlaceObject(Hits[0].pose);
            }
            else
            {
                // Fallback: place 1.5 m in front of camera so the lesson object
                // always appears even when AR plane detection hasn't kicked in yet.
                PlaceAtCameraForward();
            }
        }

        private void UpdateIndicator()
        {
            if (placementIndicator == null) return;
            if (raycastManager == null || !raycastManager.enabled)
            {
                ShowIndicator(false);
                return;
            }

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (raycastManager.Raycast(center, Hits, TrackableType.PlaneWithinPolygon))
            {
                ShowIndicator(true);
                placementIndicator.transform.SetPositionAndRotation(
                    Hits[0].pose.position, Hits[0].pose.rotation);
            }
            else
            {
                ShowIndicator(false);
            }
        }

        private void PlaceAtCameraForward()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = cam.transform.forward;
            forward.Normalize();

            Vector3 pos = cam.transform.position + forward * 1.5f;
            pos.y = cam.transform.position.y - 0.5f;

            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
            PlaceObject(new Pose(pos, rot));
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
            }
            OnObjectPlaced?.Invoke(_pendingObject, pose);
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
