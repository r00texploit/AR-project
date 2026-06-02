using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.Lessons
{
    /// <summary>
    /// Triangle lesson: three sliders control side lengths A, B, C.
    /// Mesh updates in real time; perimeter and area are recalculated and displayed.
    /// </summary>
    public class TriangleLessonController : LessonBase
    {
        [Header("3D Objects")]
        [SerializeField] private MeshFilter   triangleMeshFilter;
        [SerializeField] private LineRenderer perimeterOutline;

        [Header("UI – Sliders")]
        [SerializeField] private Slider sliderA;
        [SerializeField] private Slider sliderB;
        [SerializeField] private Slider sliderC;

        [Header("UI – Labels")]
        [SerializeField] private TMP_Text labelSideA;
        [SerializeField] private TMP_Text labelSideB;
        [SerializeField] private TMP_Text labelSideC;
        [SerializeField] private TMP_Text labelPerimeter;
        [SerializeField] private TMP_Text labelArea;
        [SerializeField] private TMP_Text labelFormula;
        [SerializeField] private TMP_Text labelExplanation;
        [SerializeField] private TMP_Text labelWarning;

        private float _sideA = 3f;
        private float _sideB = 4f;
        private float _sideC = 5f;

        private const float MinSide = 0.5f;
        private const float MaxSide = 10f;

        protected override void OnInitialize()
        {
            LessonId = "triangle";

            ConfigureSlider(sliderA, MinSide, MaxSide, _sideA, v => { _sideA = v; Validate(); });
            ConfigureSlider(sliderB, MinSide, MaxSide, _sideB, v => { _sideB = v; Validate(); });
            ConfigureSlider(sliderC, MinSide, MaxSide, _sideC, v => { _sideC = v; Validate(); });

            if (labelExplanation != null)
                labelExplanation.text =
                    "When a side grows longer,\nthe perimeter increases.";

            if (labelWarning != null)
                labelWarning.gameObject.SetActive(false);

            UpdateMesh();
        }

        protected override void OnReset()
        {
            _sideA = 3f; _sideB = 4f; _sideC = 5f;
            if (sliderA != null) sliderA.value = _sideA;
            if (sliderB != null) sliderB.value = _sideB;
            if (sliderC != null) sliderC.value = _sideC;
            UpdateMesh();
        }

        private void Validate()
        {
            bool valid = _sideA < _sideB + _sideC
                      && _sideB < _sideA + _sideC
                      && _sideC < _sideA + _sideB;

            if (labelWarning != null)
                labelWarning.gameObject.SetActive(!valid);

            if (valid) UpdateMesh();
        }

        private void UpdateMesh()
        {
            var (mesh, vA, vB, vC) = MeshGenerator.GenerateTriangle(_sideA, _sideB, _sideC);

            if (triangleMeshFilter != null)
                triangleMeshFilter.mesh = mesh;

            // Draw outline along the three edges
            if (perimeterOutline != null)
            {
                perimeterOutline.positionCount = 4;
                perimeterOutline.SetPositions(new[] { vA, vB, vC, vA });
            }

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            float perimeter = _sideA + _sideB + _sideC;
            float s         = perimeter / 2f;
            float areaArg   = s * (s - _sideA) * (s - _sideB) * (s - _sideC);
            float area      = areaArg > 0f ? Mathf.Sqrt(areaArg) : 0f;

            if (labelSideA    != null) labelSideA.text    = $"A = {_sideA:F1}";
            if (labelSideB    != null) labelSideB.text    = $"B = {_sideB:F1}";
            if (labelSideC    != null) labelSideC.text    = $"C = {_sideC:F1}";
            if (labelPerimeter != null) labelPerimeter.text =
                $"Perimeter = {perimeter:F2}";
            if (labelArea     != null) labelArea.text     = $"Area = {area:F2}";
            if (labelFormula  != null) labelFormula.text  =
                $"P = A + B + C\nP = {_sideA:F1} + {_sideB:F1} + {_sideC:F1} = {perimeter:F2}";
        }

        private static void ConfigureSlider(Slider s, float min, float max, float val,
            UnityEngine.Events.UnityAction<float> cb)
        {
            if (s == null) return;
            s.minValue = min;
            s.maxValue = max;
            s.value    = val;
            s.onValueChanged.AddListener(cb);
        }
    }
}
