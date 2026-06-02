using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREducation.Utils;

namespace AREducation.Lessons
{
    /// <summary>
    /// Cube lesson: tap a face to highlight it, slider adjusts cube size.
    /// Displays surface area and volume formulas.
    /// </summary>
    public class CubeLessonController : LessonBase
    {
        [Header("3D Objects")]
        [SerializeField] private MeshFilter   cubeMeshFilter;
        [SerializeField] private MeshRenderer cubeMeshRenderer;
        [SerializeField] private BoxCollider  cubeCollider;

        [Header("Materials")]
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material highlightMaterial;

        [Header("UI")]
        [SerializeField] private TMP_Text labelFaceInfo;
        [SerializeField] private TMP_Text labelFormula;
        [SerializeField] private TMP_Text labelSize;
        [SerializeField] private Slider   sizeSlider;

        private float _size = 1f;
        private int   _highlightedFace = -1;

        private static readonly string[] FaceNames =
            { "Front Face", "Back Face", "Left Face", "Right Face", "Top Face", "Bottom Face" };

        private static readonly Vector3[] FaceNormals =
        {
            Vector3.forward, Vector3.back,
            Vector3.left,    Vector3.right,
            Vector3.up,      Vector3.down,
        };

        protected override void OnInitialize()
        {
            LessonId = "cube";

            // Create runtime materials if none assigned
            if (defaultMaterial   == null) defaultMaterial   = CreateMat(new Color(0.3f, 0.6f, 1f));
            if (highlightMaterial == null) highlightMaterial = CreateMat(Color.yellow);

            if (sizeSlider != null)
            {
                sizeSlider.minValue = 0.2f;
                sizeSlider.maxValue = 4f;
                sizeSlider.value    = _size;
                sizeSlider.onValueChanged.AddListener(OnSizeChanged);
            }

            UpdateMesh();
            UpdateLabels();
        }

        protected override void OnReset()
        {
            _size = 1f;
            _highlightedFace = -1;
            if (sizeSlider != null) sizeSlider.value = _size;
            UpdateMesh();
            UpdateLabels();
        }

        void Update()
        {
            if (Input.touchCount != 1) return;
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;
            if (hit.collider.gameObject != gameObject) return;

            // Determine face from the hit normal in local space
            Vector3 localNorm = transform.InverseTransformDirection(hit.normal).normalized;
            _highlightedFace  = GetClosestFaceIndex(localNorm);
            UpdateLabels();
        }

        private void OnSizeChanged(float size)
        {
            _size = size;
            if (labelSize != null) labelSize.text = $"Size: {size:F2}";
            UpdateMesh();
            UpdateLabels();
        }

        private void UpdateMesh()
        {
            Mesh mesh = MeshGenerator.GenerateCube(_size);
            if (cubeMeshFilter  != null) cubeMeshFilter.mesh = mesh;
            if (cubeMeshRenderer != null)
                cubeMeshRenderer.material = defaultMaterial;
            if (cubeCollider != null)
                cubeCollider.size = Vector3.one * _size * 0.1f; // ArScale = 0.1
        }

        private void UpdateLabels()
        {
            float sa     = 6f  * _size * _size;
            float volume = _size * _size * _size;

            string faceLine = _highlightedFace >= 0
                ? $"Selected: {FaceNames[_highlightedFace]}\nFace Area = {_size * _size:F2}"
                : "Tap a face to inspect it.";

            if (labelFaceInfo != null) labelFaceInfo.text = faceLine;
            if (labelFormula  != null) labelFormula.text  =
                $"Surface Area = 6 × s² = {sa:F2}\nVolume = s³ = {volume:F2}";
            if (labelSize     != null) labelSize.text     = $"Size: {_size:F2}";
        }

        private int GetClosestFaceIndex(Vector3 localNorm)
        {
            int   best  = 0;
            float bestD = float.MinValue;
            for (int i = 0; i < FaceNormals.Length; i++)
            {
                float d = Vector3.Dot(localNorm, FaceNormals[i]);
                if (d > bestD) { bestD = d; best = i; }
            }
            return best;
        }

        private static Material CreateMat(Color color)
        {
            var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Hidden/InternalErrorShader"));
            mat.color = color;
            return mat;
        }
    }
}
