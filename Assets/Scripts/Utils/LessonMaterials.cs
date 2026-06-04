using UnityEngine;

namespace AREducation.Utils
{
    /// <summary>
    /// Creates and manages materials for lesson objects with educational aesthetics.
    /// </summary>
    public static class LessonMaterials
    {
        // Color palettes for different lessons
        private static readonly Color TrianglePrimary = new Color(0.2f, 0.6f, 0.9f, 1f);   // Educational blue
        private static readonly Color TriangleSecondary = new Color(0.1f, 0.4f, 0.7f, 1f); // Darker blue
        private static readonly Color TriangleHighlight = new Color(1f, 0.8f, 0.2f, 1f);  // Yellow highlight
        private static readonly Color TriangleOutline = new Color(1f, 0.3f, 0.2f, 1f);     // Red outline

        private static readonly Color CubePrimary = new Color(0.3f, 0.7f, 0.4f, 1f);     // Math green
        private static readonly Color CubeSecondary = new Color(0.2f, 0.5f, 0.3f, 1f);    // Darker green
        private static readonly Color CubeHighlight = new Color(1f, 0.9f, 0.2f, 1f);     // Yellow highlight
        private static readonly Color[] CubeFaceColors = new Color[]
        {
            new Color(0.3f, 0.7f, 0.4f, 1f),  // Front - Green
            new Color(0.4f, 0.6f, 0.3f, 1f),  // Back - Olive
            new Color(0.2f, 0.7f, 0.5f, 1f),  // Left - Teal
            new Color(0.3f, 0.5f, 0.7f, 1f),  // Right - Blue-green
            new Color(0.5f, 0.7f, 0.3f, 1f),  // Top - Lime
            new Color(0.3f, 0.4f, 0.7f, 1f),  // Bottom - Purple-blue
        };

        private static readonly Color PhysicsPrimary = new Color(0.9f, 0.3f, 0.2f, 1f);  // Physics red
        private static readonly Color PhysicsSecondary = new Color(0.7f, 0.2f, 0.1f, 1f);  // Darker red
        private static readonly Color TrailColor = new Color(1f, 0.5f, 0.3f, 0.6f);      // Orange trail

        /// <summary>
        /// Creates a gradient material for the triangle lesson.
        /// </summary>
        public static Material CreateTriangleMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "TriangleLesson_Material";

            // Educational blue with slight gloss
            mat.SetColor("_Color", TrianglePrimary);
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Glossiness", 0.6f);

            // Enable emission for slight glow effect
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", TriangleSecondary * 0.2f);

            return mat;
        }

        /// <summary>
        /// Creates a highlight material for triangle when selected.
        /// </summary>
        public static Material CreateTriangleHighlightMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "TriangleLesson_Highlight";

            mat.SetColor("_Color", TriangleHighlight);
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", TriangleHighlight * 0.3f);

            return mat;
        }

        /// <summary>
        /// Creates materials for each cube face (different colors for educational clarity).
        /// </summary>
        public static Material[] CreateCubeFaceMaterials()
        {
            Material[] materials = new Material[6];
            string[] faceNames = { "Front", "Back", "Left", "Right", "Top", "Bottom" };

            for (int i = 0; i < 6; i++)
            {
                materials[i] = new Material(Shader.Find("Standard"));
                materials[i].name = $"CubeLesson_{faceNames[i]}";
                materials[i].SetColor("_Color", CubeFaceColors[i]);
                materials[i].SetFloat("_Metallic", 0.1f);
                materials[i].SetFloat("_Glossiness", 0.5f);

                // Subtle emission for visibility in AR
                materials[i].EnableKeyword("_EMISSION");
                materials[i].SetColor("_EmissionColor", CubeFaceColors[i] * 0.15f);
            }

            return materials;
        }

        /// <summary>
        /// Creates a highlight material for cube faces.
        /// </summary>
        public static Material CreateCubeHighlightMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "CubeLesson_Highlight";

            mat.SetColor("_Color", CubeHighlight);
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Glossiness", 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", CubeHighlight * 0.5f);

            return mat;
        }

        /// <summary>
        /// Creates a material for the physics ball.
        /// </summary>
        public static Material CreatePhysicsBallMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "PhysicsLesson_Ball";

            // Physics red with high gloss for "ball" appearance
            mat.SetColor("_Color", PhysicsPrimary);
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Glossiness", 0.85f);

            // Stronger emission for visibility during motion
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", PhysicsSecondary * 0.4f);

            return mat;
        }

        /// <summary>
        /// Creates a material for the physics trail.
        /// </summary>
        public static Material CreatePhysicsTrailMaterial()
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.name = "PhysicsLesson_Trail";

            // Transparent gradient texture effect
            mat.SetColor("_Color", TrailColor);

            return mat;
        }

        /// <summary>
        /// Creates a wireframe/outline material for edges.
        /// </summary>
        public static Material CreateOutlineMaterial(Color color, float thickness = 0.02f)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.name = "LessonOutline";
            mat.SetColor("_Color", color);
            return mat;
        }

        /// <summary>
        /// Creates a ground plane shadow material.
        /// </summary>
        public static Material CreateShadowMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "LessonShadow";

            mat.SetColor("_Color", new Color(0, 0, 0, 0.3f));
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 0f);

            // Use transparent rendering mode
            mat.SetFloat("_Mode", 3f); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            return mat;
        }

        /// <summary>
        /// Applies a simple checkerboard texture pattern to a material.
        /// </summary>
        public static void ApplyCheckerboardPattern(Material mat, Color color1, Color color2, int size = 64)
        {
            Texture2D tex = new Texture2D(size, size);
            tex.name = "CheckerboardTexture";

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool check = ((x / 8) + (y / 8)) % 2 == 0;
                    tex.SetPixel(x, y, check ? color1 : color2);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            mat.SetTexture("_MainTex", tex);
        }
    }
}
