using UnityEngine;
using UnityEditor;
using AREducation.Lessons;
using AREducation.AR;

namespace AREducation.Editor
{
    /// <summary>
    /// Builds lesson prefabs for the ModelRegistry.
    /// Run from menu: AR Education → Build Lesson Prefabs
    /// </summary>
    public static class LessonPrefabBuilder
    {
        [MenuItem("AR Education/Build Lesson Prefabs")]
        public static void BuildAllPrefabs()
        {
            string prefabPath = "Assets/Prefabs/Lessons";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Lessons");
            }

            // Build Triangle Lesson Prefab
            BuildTrianglePrefab($"{prefabPath}/TriangleLesson.prefab");

            // Build Cube Lesson Prefab
            BuildCubePrefab($"{prefabPath}/CubeLesson.prefab");

            // Build Physics Lesson Prefab
            BuildPhysicsPrefab($"{prefabPath}/PhysicsLesson.prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("[LessonPrefabBuilder] All lesson prefabs created successfully!");
        }

        private static void BuildTrianglePrefab(string path)
        {
            // Create root GameObject
            GameObject root = new GameObject("TriangleLesson");

            // Add TriangleLessonController
            var controller = root.AddComponent<TriangleLessonController>();

            // Create Triangle Mesh GO
            GameObject triangleGO = new GameObject("TriangleMesh");
            triangleGO.transform.SetParent(root.transform);
            var meshFilter = triangleGO.AddComponent<MeshFilter>();
            var meshRenderer = triangleGO.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = new Color(0.3f, 0.6f, 1f);

            // Create Line Renderer for perimeter
            GameObject outlineGO = new GameObject("PerimeterOutline");
            outlineGO.transform.SetParent(root.transform);
            var lineRenderer = outlineGO.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.useWorldSpace = true;

            // Assign via reflection or serialize field manually
            var so = new SerializedObject(controller);
            so.FindProperty("triangleMeshFilter").objectReferenceValue = meshFilter;
            so.FindProperty("perimeterOutline").objectReferenceValue = lineRenderer;
            so.ApplyModifiedProperties();

            // Create prefab
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[LessonPrefabBuilder] Created: {path}");
        }

        private static void BuildCubePrefab(string path)
        {
            GameObject root = new GameObject("CubeLesson");

            // Add CubeLessonController
            var controller = root.AddComponent<CubeLessonController>();

            // Create Cube Mesh GO
            GameObject cubeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeGO.name = "CubeMesh";
            cubeGO.transform.SetParent(root.transform);
            var meshFilter = cubeGO.GetComponent<MeshFilter>();
            var meshRenderer = cubeGO.GetComponent<MeshRenderer>();

            // Create materials
            var defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = new Color(0.3f, 0.6f, 1f);
            meshRenderer.material = defaultMat;

            var highlightMat = new Material(Shader.Find("Standard"));
            highlightMat.color = Color.yellow;

            // Get collider
            var collider = cubeGO.GetComponent<BoxCollider>();

            // Assign via serialized object
            var so = new SerializedObject(controller);
            so.FindProperty("cubeMeshFilter").objectReferenceValue = meshFilter;
            so.FindProperty("cubeMeshRenderer").objectReferenceValue = meshRenderer;
            so.FindProperty("cubeCollider").objectReferenceValue = collider;
            so.FindProperty("defaultMaterial").objectReferenceValue = defaultMat;
            so.FindProperty("highlightMaterial").objectReferenceValue = highlightMat;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[LessonPrefabBuilder] Created: {path}");
        }

        private static void BuildPhysicsPrefab(string path)
        {
            GameObject root = new GameObject("PhysicsLesson");

            // Add PhysicsLessonController
            var controller = root.AddComponent<PhysicsLessonController>();

            // Create Ball
            GameObject ballGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGO.name = "PhysicsBall";
            ballGO.transform.SetParent(root.transform);
            ballGO.transform.localScale = Vector3.one * 0.1f;

            var ballRenderer = ballGO.GetComponent<MeshRenderer>();
            ballRenderer.material = new Material(Shader.Find("Standard"));
            ballRenderer.material.color = new Color(1f, 0.3f, 0.3f);

            // Remove collider - not needed for visual
            Object.DestroyImmediate(ballGO.GetComponent<Collider>());

            // Create Trail Renderer
            var trailRenderer = ballGO.AddComponent<TrailRenderer>();
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.startColor = new Color(1f, 0.3f, 0.3f, 0.5f);
            trailRenderer.endColor = new Color(1f, 0.3f, 0.3f, 0f);
            trailRenderer.startWidth = 0.05f;
            trailRenderer.endWidth = 0.01f;
            trailRenderer.time = 2f;

            // Assign via serialized object
            var so = new SerializedObject(controller);
            so.FindProperty("ballTransform").objectReferenceValue = ballGO.transform;
            so.FindProperty("ballTrail").objectReferenceValue = trailRenderer;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[LessonPrefabBuilder] Created: {path}");
        }
    }
}
