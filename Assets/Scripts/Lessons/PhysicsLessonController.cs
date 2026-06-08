using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.Lessons
{
    /// <summary>
    /// Pendulum lesson — simulates a Simple Pendulum using the SHM approximation.
    /// θ(t) = θ₀ · cos(ω·t),  ω = √(g / L),  T = 2π√(L/g)
    ///
    /// Reuses the same serialized fields as the old linear-physics lesson so that
    /// existing scene references remain valid without any scene changes.
    /// </summary>
    public class PhysicsLessonController : LessonBase
    {
        [Header("3D Objects (from scene)")]
        [SerializeField] private Transform    ballTransform;    // pendulum bob
        [SerializeField] private TrailRenderer ballTrail;       // visual trail
        [SerializeField] private MeshRenderer  ballRenderer;

        [Header("Visual Effects")]
        [SerializeField] private bool enableGlow  = true;
        [SerializeField] private bool enableTrail = true;

        [Header("UI — Labels (repurposed)")]
        [SerializeField] private TMP_Text labelSpeed;     // → Length
        [SerializeField] private TMP_Text labelTime;      // → Period
        [SerializeField] private TMP_Text labelDistance;  // → Angle
        [SerializeField] private TMP_Text labelFormula;   // → T = 2π√(L/g)

        [Header("UI — Buttons (repurposed)")]
        [SerializeField] private Button   btnStartStop;
        [SerializeField] private Button   btnReset;
        [SerializeField] private Button   btnSpeedUp;    // → gravity up
        [SerializeField] private Button   btnSlowDown;   // → gravity down
        [SerializeField] private TMP_Text btnStartStopLabel;

        // Pendulum parameters
        private float _length    = 1.5f;   // lesson units  (×0.1 = metres in AR)
        private float _gravity   = 9.81f;  // m/s²
        private float _initAngle = 30f;    // degrees
        private float _elapsed;
        private bool  _isRunning;

        private const float ArScale   = 0.1f;
        private const float MinLength = 0.5f;
        private const float MaxLength = 3.0f;
        private const float MinGravity = 1f;
        private const float MaxGravity = 20f;
        private const float GravityStep = 1f;

        // Pivot offset above the lesson GO origin (in local space)
        private Vector3 _pivotLocal = new Vector3(0f, 0.45f, 0f);

        // Runtime string renderer
        private LineRenderer _stringLR;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        protected override void OnInitialize()
        {
            LessonId = "pendulum";

            SetupBobMaterial();
            SetupTrail();
            CreateStringRenderer();

            btnStartStop?.onClick.AddListener(ToggleSimulation);
            btnReset?.onClick.AddListener(ResetPendulum);
            btnSpeedUp?.onClick.AddListener(() => AdjustGravity(+GravityStep));
            btnSlowDown?.onClick.AddListener(() => AdjustGravity(-GravityStep));

            // Update button labels to match new semantics
            if (btnStartStopLabel != null) btnStartStopLabel.text = "Start";
            TryRelabelButton(btnSpeedUp,   "Gravity +");
            TryRelabelButton(btnSlowDown,  "Gravity −");

            ResetPendulum();
        }

        protected override void OnReset() => ResetPendulum();

        // ── Update ────────────────────────────────────────────────────────────

        void Update()
        {
            if (!_isRunning) return;

            _elapsed += Time.deltaTime;

            float omega   = Mathf.Sqrt(_gravity / (_length * ArScale));
            float thetaRad = _initAngle * Mathf.Deg2Rad * Mathf.Cos(omega * _elapsed);

            // Bob position in local space relative to lesson GO
            float bobX = _pivotLocal.x + (_length * ArScale) * Mathf.Sin(thetaRad);
            float bobY = _pivotLocal.y - (_length * ArScale) * Mathf.Cos(thetaRad);

            if (ballTransform != null)
                ballTransform.localPosition = new Vector3(bobX, bobY, 0f);

            UpdateStringVisual();
            UpdateLabels(thetaRad * Mathf.Rad2Deg);
        }

        // ── Controls ─────────────────────────────────────────────────────────

        private void ToggleSimulation()
        {
            _isRunning = !_isRunning;
            if (btnStartStopLabel != null)
                btnStartStopLabel.text = _isRunning ? "Stop" : "Start";
        }

        private void ResetPendulum()
        {
            _isRunning = false;
            _elapsed   = 0f;

            if (btnStartStopLabel != null) btnStartStopLabel.text = "Start";

            // Place bob at rest position (hanging straight down)
            if (ballTransform != null)
                ballTransform.localPosition = new Vector3(
                    _pivotLocal.x,
                    _pivotLocal.y - _length * ArScale,
                    0f);

            ballTrail?.Clear();
            UpdateStringVisual();
            UpdateLabels(0f);
        }

        private void AdjustGravity(float delta)
        {
            _gravity = Mathf.Clamp(_gravity + delta, MinGravity, MaxGravity);
            _elapsed = 0f; // restart so motion stays coherent
            UpdateLabels(0f);
        }

        // ── Visuals ───────────────────────────────────────────────────────────

        private void SetupBobMaterial()
        {
            if (ballRenderer == null && ballTransform != null)
                ballRenderer = ballTransform.GetComponent<MeshRenderer>();

            if (ballRenderer != null && enableGlow)
                ballRenderer.material = LessonMaterials.CreatePhysicsBallMaterial();
        }

        private void SetupTrail()
        {
            if (ballTrail != null && enableTrail)
            {
                ballTrail.material    = LessonMaterials.CreatePhysicsTrailMaterial();
                ballTrail.startWidth  = 0.04f;
                ballTrail.endWidth    = 0.005f;
                ballTrail.time        = 3f;
            }
        }

        private void CreateStringRenderer()
        {
            _stringLR               = gameObject.AddComponent<LineRenderer>();
            _stringLR.positionCount = 2;
            _stringLR.useWorldSpace = true;
            _stringLR.material      = LessonMaterials.CreateOutlineMaterial(new Color(0.9f, 0.9f, 0.8f));
            _stringLR.startWidth    = 0.005f;
            _stringLR.endWidth      = 0.005f;
            _stringLR.numCapVertices = 0;
        }

        private void UpdateStringVisual()
        {
            if (_stringLR == null) return;
            Vector3 pivotWorld = transform.TransformPoint(_pivotLocal);
            Vector3 bobWorld   = ballTransform != null
                ? ballTransform.position
                : transform.TransformPoint(_pivotLocal + Vector3.down * _length * ArScale);
            _stringLR.SetPosition(0, pivotWorld);
            _stringLR.SetPosition(1, bobWorld);
        }

        private void UpdateLabels(float currentAngleDeg)
        {
            float L      = _length * ArScale;
            float period = Mathf.Approximately(_gravity, 0f)
                ? 0f
                : 2f * Mathf.PI * Mathf.Sqrt(L / _gravity);

            if (labelSpeed)    labelSpeed.text    = $"Length:  {L:F2} m";
            if (labelTime)     labelTime.text     = $"Period:  {period:F2} s";
            if (labelDistance) labelDistance.text = $"Angle:   {currentAngleDeg:F1}°";
            if (labelFormula)  labelFormula.text  =
                $"T = 2π√(L/g)\nT = 2π√({L:F2}/{_gravity:F1}) = {period:F2} s";
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void TryRelabelButton(Button btn, string label)
        {
            if (btn == null) return;
            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = label;
        }
    }
}
