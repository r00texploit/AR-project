using UnityEngine;
using UnityEngine.EventSystems;

namespace AREducation.AR
{
    /// <summary>
    /// Attaches to a placed AR object.
    /// 1 finger = drag (horizontal plane).
    /// 2 fingers = pinch-to-scale + twist-to-rotate.
    /// </summary>
    public class ARObjectManipulator : MonoBehaviour
    {
        [SerializeField] private float dragSensitivity   = 0.003f;
        [SerializeField] private float scaleSensitivity  = 0.003f;
        [SerializeField] private float rotateSensitivity = 0.4f;
        [SerializeField] private float minScale = 0.15f;
        [SerializeField] private float maxScale = 4f;

        private Camera _arCamera;
        private bool   _enabled = true;

        // Drag state
        private Vector2 _lastDragPos;
        private bool    _dragging;

        void Start() => _arCamera = Camera.main;

        public void SetEnabled(bool value) => _enabled = value;

        void Update()
        {
            if (!_enabled) return;
            if (IsAnyTouchOverUI()) return;

            if (Input.touchCount == 1)
                HandleDrag();
            else if (Input.touchCount == 2)
                HandlePinchAndRotate();
        }

        private void HandleDrag()
        {
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    _lastDragPos = t.position;
                    _dragging = true;
                    break;
                case TouchPhase.Moved when _dragging:
                    Vector2 delta = t.position - _lastDragPos;
                    if (_arCamera != null)
                    {
                        Vector3 worldDelta =
                            _arCamera.transform.right  * (delta.x * dragSensitivity) +
                            _arCamera.transform.up     * (delta.y * dragSensitivity);
                        worldDelta.y = 0f; // constrain to horizontal
                        transform.position += worldDelta;
                    }
                    _lastDragPos = t.position;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _dragging = false;
                    break;
            }
        }

        private void HandlePinchAndRotate()
        {
            _dragging = false; // cancel any drag if second finger appears

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevMag = (prev0 - prev1).magnitude;
            float currMag = (t0.position - t1.position).magnitude;
            float pinchDelta = currMag - prevMag;

            // Scale
            float newScale = transform.localScale.x * (1f + pinchDelta * scaleSensitivity);
            newScale = Mathf.Clamp(newScale, minScale, maxScale);
            transform.localScale = Vector3.one * newScale;

            // Rotation — signed angle between previous and current touch-vector
            float angle = Vector2.SignedAngle(prev1 - prev0, t1.position - t0.position);
            transform.Rotate(Vector3.up, -angle * rotateSensitivity, Space.World);
        }

        private static bool IsAnyTouchOverUI()
        {
            if (EventSystem.current == null) return false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;
            }
            return false;
        }
    }
}
