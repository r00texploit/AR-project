using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREducation.Lessons
{
    /// <summary>
    /// Physics lesson: simulates constant-velocity ball motion.
    /// Formula: d = v × t
    /// </summary>
    public class PhysicsLessonController : LessonBase
    {
        [Header("3D Objects")]
        [SerializeField] private Transform    ballTransform;
        [SerializeField] private TrailRenderer ballTrail;

        [Header("UI – Display Labels")]
        [SerializeField] private TMP_Text labelSpeed;
        [SerializeField] private TMP_Text labelTime;
        [SerializeField] private TMP_Text labelDistance;
        [SerializeField] private TMP_Text labelFormula;

        [Header("UI – Controls")]
        [SerializeField] private Button    btnStartStop;
        [SerializeField] private Button    btnReset;
        [SerializeField] private Button    btnSpeedUp;
        [SerializeField] private Button    btnSlowDown;
        [SerializeField] private TMP_Text  btnStartStopLabel;

        private float   _speed        = 2f;
        private float   _elapsed      = 0f;
        private float   _distance     = 0f;
        private bool    _isMoving     = false;
        private Vector3 _startPos;

        private const float MaxDistance = 3f;   // metres in AR space
        private const float MinSpeed    = 0.5f;
        private const float MaxSpeed    = 10f;
        private const float SpeedStep   = 0.5f;

        protected override void OnInitialize()
        {
            LessonId = "physics";

            if (ballTransform != null)
                _startPos = ballTransform.localPosition;

            btnStartStop?.onClick.AddListener(ToggleMovement);
            btnReset?.onClick.AddListener(ResetBall);
            btnSpeedUp?.onClick.AddListener(() => AdjustSpeed(+SpeedStep));
            btnSlowDown?.onClick.AddListener(() => AdjustSpeed(-SpeedStep));

            UpdateUI();
        }

        protected override void OnReset() => ResetBall();

        void Update()
        {
            if (!_isMoving) return;

            _elapsed  += Time.deltaTime;
            _distance  = _speed * _elapsed;

            if (ballTransform != null)
                ballTransform.localPosition = _startPos + Vector3.right * _distance * 0.1f;

            if (_distance >= MaxDistance)
                ResetBall();
            else
                UpdateUI();
        }

        private void ToggleMovement()
        {
            _isMoving = !_isMoving;
            if (btnStartStopLabel != null)
                btnStartStopLabel.text = _isMoving ? "Stop" : "Start";
        }

        private void ResetBall()
        {
            _isMoving = false;
            _elapsed  = 0f;
            _distance = 0f;

            if (ballTransform != null)
                ballTransform.localPosition = _startPos;

            ballTrail?.Clear();

            if (btnStartStopLabel != null)
                btnStartStopLabel.text = "Start";

            UpdateUI();
        }

        private void AdjustSpeed(float delta)
        {
            _speed = Mathf.Clamp(_speed + delta, MinSpeed, MaxSpeed);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (labelSpeed)
                labelSpeed.text = $"Speed:  {_speed:F1} m/s";
            if (labelTime)
                labelTime.text = $"Time:   {_elapsed:F2} s";
            if (labelDistance)
                labelDistance.text = $"Distance: {_distance:F2} m";
            if (labelFormula)
                labelFormula.text =
                    $"d = v × t\nd = {_speed:F1} × {_elapsed:F2} = {_distance:F2}";
        }
    }
}
