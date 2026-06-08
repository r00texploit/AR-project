using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.Lessons
{
    /// <summary>
    /// Unit Circle lesson — visualises sine and cosine on an interactive circle.
    /// If no UI slider is wired in the scene, the angle auto-animates at 30°/s.
    /// All 3D geometry (circle, axes, component lines) is built procedurally at runtime.
    /// </summary>
    public class UnitCircleLessonController : LessonBase
    {
        [Header("Optional UI — leave null for world-space auto-display")]
        [SerializeField] private Slider   sliderAngle;
        [SerializeField] private TMP_Text labelSin;
        [SerializeField] private TMP_Text labelCos;
        [SerializeField] private TMP_Text labelAngle;
        [SerializeField] private TMP_Text labelFormula;

        // AR radius of the unit circle (world-space metres)
        private const float Radius      = 0.22f;
        private const float AutoSpeed   = 30f;   // degrees per second when no slider

        // Runtime geometry
        private LineRenderer _circleLR;
        private LineRenderer _xAxisLR;
        private LineRenderer _yAxisLR;
        private LineRenderer _radiusLR;
        private LineRenderer _sinLR;
        private LineRenderer _cosLR;
        private Transform    _pointMarker;

        // World-space labels (created when no overlay UI is wired)
        private TMP_Text _wsAngle;
        private TMP_Text _wsSin;
        private TMP_Text _wsCos;
        private TMP_Text _wsFormula;

        private float _angleDeg;
        private bool  _hasSlider;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        protected override void OnInitialize()
        {
            LessonId = "unit_circle";
            CreateGeometry();
            CreateWorldSpaceLabels();

            if (sliderAngle != null)
            {
                sliderAngle.minValue = 0f;
                sliderAngle.maxValue = 360f;
                sliderAngle.value    = 0f;
                sliderAngle.onValueChanged.AddListener(v => { _angleDeg = v; Refresh(); });
                _hasSlider = true;
            }

            Refresh();
        }

        protected override void OnReset()
        {
            _angleDeg = 0f;
            if (sliderAngle != null) sliderAngle.value = 0f;
            Refresh();
        }

        void Update()
        {
            if (!_hasSlider && IsInitialized)
            {
                _angleDeg = (_angleDeg + AutoSpeed * Time.deltaTime) % 360f;
                Refresh();
            }
        }

        // ── 3-D Geometry ─────────────────────────────────────────────────────────

        private void CreateGeometry()
        {
            // Circle outline (64 segments)
            _circleLR = MakeLine("Circle", new Color(0.5f, 0.6f, 1f), 0.005f);
            _circleLR.positionCount = 65;
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI * 2f / 64f;
                _circleLR.SetPosition(i, new Vector3(Mathf.Cos(a) * Radius, Mathf.Sin(a) * Radius, 0));
            }

            // Axes
            _xAxisLR = MakeLine("XAxis", new Color(0.9f, 0.3f, 0.3f), 0.003f);
            _xAxisLR.SetPositions(new[]
            {
                new Vector3(-Radius * 1.25f, 0, 0),
                new Vector3( Radius * 1.25f, 0, 0)
            });

            _yAxisLR = MakeLine("YAxis", new Color(0.3f, 0.9f, 0.3f), 0.003f);
            _yAxisLR.SetPositions(new[]
            {
                new Vector3(0, -Radius * 1.25f, 0),
                new Vector3(0,  Radius * 1.25f, 0)
            });

            // Dynamic lines (updated each frame)
            _radiusLR = MakeLine("Radius",  new Color(1f, 1f, 0.4f),    0.006f);
            _sinLR    = MakeLine("SinLine", new Color(0.3f, 0.9f, 0.9f), 0.007f);
            _cosLR    = MakeLine("CosLine", new Color(1f, 0.6f, 0.3f),   0.007f);

            foreach (var lr in new[] { _radiusLR, _sinLR, _cosLR })
                lr.positionCount = 2;

            // Marker sphere on circle
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "PointMarker";
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * 0.03f;
            Destroy(sphere.GetComponent<Collider>());
            MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
            if (mr != null) mr.material = LessonMaterials.CreateOutlineMaterial(Color.white);
            _pointMarker = sphere.transform;
        }

        private LineRenderer MakeLine(string goName, Color color, float width)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace    = false;
            lr.material         = LessonMaterials.CreateOutlineMaterial(color);
            lr.startWidth       = width;
            lr.endWidth         = width;
            lr.positionCount    = 2;
            lr.numCapVertices   = 4;
            return lr;
        }

        // ── World-Space Labels ───────────────────────────────────────────────────

        private void CreateWorldSpaceLabels()
        {
            // Skip if overlay UI is fully wired
            if (labelSin != null && labelCos != null && labelAngle != null) return;

            var canvasGO = new GameObject("WorldUI");
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = new Vector3(Radius + 0.05f, Radius * 0.5f, 0);
            canvasGO.transform.localScale    = Vector3.one * 0.001f;

            Canvas c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasRenderer>();
            RectTransform crt = canvasGO.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(320, 220);

            _wsAngle   = MakeLabel(canvasGO, "θ = 0.0°",          new Vector2(160, 185));
            _wsSin     = MakeLabel(canvasGO, "sin = 0.000",        new Vector2(160, 140));
            _wsCos     = MakeLabel(canvasGO, "cos = 1.000",        new Vector2(160, 95));
            _wsFormula = MakeLabel(canvasGO, "sin²θ + cos²θ = 1",  new Vector2(160, 40));
            _wsFormula.fontSize = 22;
        }

        private static TMP_Text MakeLabel(GameObject parent, string text, Vector2 anchoredPos)
        {
            var go = new GameObject(text);
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta       = new Vector2(300, 40);
            rt.anchoredPosition = anchoredPos;
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 28;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        // ── Visuals ──────────────────────────────────────────────────────────────

        private void Refresh()
        {
            float rad = _angleDeg * Mathf.Deg2Rad;
            float sinV = Mathf.Sin(rad);
            float cosV = Mathf.Cos(rad);

            Vector3 pt   = new Vector3(cosV * Radius, sinV * Radius, 0);
            Vector3 xPt  = new Vector3(cosV * Radius, 0, 0);

            // Radius line: origin → point
            _radiusLR.SetPosition(0, Vector3.zero);
            _radiusLR.SetPosition(1, pt);

            // Sin line: (cosV*R, 0) → point   (vertical component)
            _sinLR.SetPosition(0, xPt);
            _sinLR.SetPosition(1, pt);

            // Cos line: origin → (cosV*R, 0)   (horizontal component)
            _cosLR.SetPosition(0, Vector3.zero);
            _cosLR.SetPosition(1, xPt);

            // Marker
            if (_pointMarker != null) _pointMarker.localPosition = pt;

            // Labels
            string aStr = $"θ = {_angleDeg:F1}°";
            string sStr = $"sin = {sinV:F3}";
            string cStr = $"cos = {cosV:F3}";
            const string fStr = "sin²θ + cos²θ = 1";

            if (labelAngle   != null) labelAngle.text   = aStr;
            if (labelSin     != null) labelSin.text     = sStr;
            if (labelCos     != null) labelCos.text     = cStr;
            if (labelFormula != null) labelFormula.text = fStr;

            if (_wsAngle   != null) _wsAngle.text   = aStr;
            if (_wsSin     != null) _wsSin.text     = sStr;
            if (_wsCos     != null) _wsCos.text     = cStr;
            if (_wsFormula != null) _wsFormula.text  = fStr;
        }
    }
}
