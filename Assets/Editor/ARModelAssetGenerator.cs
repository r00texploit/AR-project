using System.IO;
using AREducation.Utils;
using UnityEditor;
using UnityEngine;

namespace AREducation.Editor
{
    /// <summary>
    /// Generates the Unity mesh/material assets used by the AR lesson prefabs.
    /// Run from menu: AR Education -> Generate AR Model Assets
    /// </summary>
    public static class ARModelAssetGenerator
    {
        public const string ModelDirectory = "Assets/Models/Generated";
        public const string TriangleMeshPath = ModelDirectory + "/TrianglePrism.asset";
        public const string CubeMeshPath = ModelDirectory + "/CubeSixFaces.asset";
        public const string PhysicsBallMeshPath = ModelDirectory + "/PhysicsBall.asset";
        public const string PhysicsTrackMeshPath = ModelDirectory + "/PhysicsTrack.asset";

        [MenuItem("AR Education/Generate AR Model Assets")]
        public static void GenerateAll()
        {
            EnsureDirectory();

            var (triangleMesh, _, _, _) = MeshGenerator.GenerateTriangle(3f, 4f, 5f);
            SaveMesh(triangleMesh, TriangleMeshPath);
            SaveMesh(MeshGenerator.GenerateCube(1f), CubeMeshPath);
            SaveMesh(CreateSphereMesh(), PhysicsBallMeshPath);
            SaveMesh(CreateTrackMesh(), PhysicsTrackMeshPath);

            SaveMaterial(LessonMaterials.CreateTriangleMaterial(), ModelDirectory + "/TriangleLesson.mat");
            SaveMaterial(LessonMaterials.CreateCubeFaceMaterials()[0], ModelDirectory + "/CubeDefault.mat");
            SaveMaterial(LessonMaterials.CreateCubeHighlightMaterial(), ModelDirectory + "/CubeHighlight.mat");
            SaveMaterial(LessonMaterials.CreatePhysicsBallMaterial(), ModelDirectory + "/PhysicsBall.mat");
            SaveMaterial(CreateTrackMaterial(), ModelDirectory + "/PhysicsTrack.mat");
            SaveMaterial(LessonMaterials.CreatePhysicsTrailMaterial(), ModelDirectory + "/PhysicsTrail.mat");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ARModelAssetGenerator] Generated AR lesson model assets.");
        }

        public static Mesh LoadMesh(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        public static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{ModelDirectory}/{name}.mat");
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Models"))
                AssetDatabase.CreateFolder("Assets", "Models");
            if (!AssetDatabase.IsValidFolder(ModelDirectory))
                AssetDatabase.CreateFolder("Assets/Models", "Generated");
        }

        private static void SaveMesh(Mesh mesh, string path)
        {
            mesh.name = Path.GetFileNameWithoutExtension(path);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                return;
            }

            AssetDatabase.CreateAsset(mesh, path);
        }

        private static void SaveMaterial(Material material, string path)
        {
            material.name = Path.GetFileNameWithoutExtension(path);
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(material, existing);
                return;
            }

            AssetDatabase.CreateAsset(material, path);
        }

        private static Mesh CreateSphereMesh()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = Object.Instantiate(sphere.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(sphere);
            mesh.name = "PhysicsBall";
            return mesh;
        }

        private static Mesh CreateTrackMesh()
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh mesh = Object.Instantiate(cylinder.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(cylinder);
            mesh.name = "PhysicsTrack";
            return mesh;
        }

        private static Material CreateTrackMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.name = "PhysicsTrack";
            material.color = new Color(0.4f, 0.4f, 0.5f);
            material.SetFloat("_Glossiness", 0.35f);
            return material;
        }
    }
}
