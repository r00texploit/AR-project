using AREducation.AR;
using AREducation.Lessons;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ModelAssetContractTests
{
    [Test]
    public void GeneratedMeshesMatchLessonRequirements()
    {
        Mesh triangle = LoadAsset<Mesh>("Assets/Models/Generated/TrianglePrism.asset");
        Assert.Greater(triangle.vertexCount, 0);
        Assert.Greater(triangle.triangles.Length, 0);
        Assert.Greater(triangle.bounds.size.x, 0f);
        Assert.Greater(triangle.bounds.size.z, 0f);

        Mesh cube = LoadAsset<Mesh>("Assets/Models/Generated/CubeSixFaces.asset");
        Assert.AreEqual(6, cube.subMeshCount);
        Assert.AreEqual(24, cube.vertexCount);
        Assert.Greater(cube.bounds.size.x, 0f);
        Assert.Greater(cube.bounds.size.y, 0f);
        Assert.Greater(cube.bounds.size.z, 0f);

        Mesh ball = LoadAsset<Mesh>("Assets/Models/Generated/PhysicsBall.asset");
        Assert.Greater(ball.vertexCount, 0);
        Assert.Greater(ball.bounds.size.x, 0f);
        Assert.Greater(ball.bounds.size.y, 0f);
        Assert.Greater(ball.bounds.size.z, 0f);

        Mesh track = LoadAsset<Mesh>("Assets/Models/Generated/PhysicsTrack.asset");
        Assert.Greater(track.vertexCount, 0);
        Assert.Greater(track.bounds.size.y, 0f);
    }

    [Test]
    public void GeneratedMaterialsExistForEveryModelRole()
    {
        string[] materialPaths =
        {
            "Assets/Models/Generated/TriangleLesson.mat",
            "Assets/Models/Generated/CubeDefault.mat",
            "Assets/Models/Generated/CubeHighlight.mat",
            "Assets/Models/Generated/PhysicsBall.mat",
            "Assets/Models/Generated/PhysicsTrack.mat",
            "Assets/Models/Generated/PhysicsTrail.mat"
        };

        foreach (string path in materialPaths)
        {
            Material material = LoadAsset<Material>(path);
            Assert.NotNull(material.shader, $"{path} must have a shader assigned.");
        }
    }

    [Test]
    public void LessonPrefabsContainExpectedModelComponents()
    {
        GameObject triangle = LoadAsset<GameObject>("Assets/Prefabs/Lessons/TriangleLesson.prefab");
        Assert.NotNull(triangle.GetComponent<TriangleLessonController>());
        Assert.NotNull(triangle.GetComponent<ARObjectManipulator>());
        Assert.NotNull(triangle.transform.Find("TriangleMesh")?.GetComponent<MeshFilter>()?.sharedMesh);
        Assert.NotNull(triangle.transform.Find("PerimeterOutline")?.GetComponent<LineRenderer>());

        GameObject cube = LoadAsset<GameObject>("Assets/Prefabs/Lessons/CubeLesson.prefab");
        Assert.NotNull(cube.GetComponent<CubeLessonController>());
        Assert.NotNull(cube.GetComponent<ARObjectManipulator>());
        Assert.NotNull(cube.transform.Find("CubeMesh")?.GetComponent<MeshFilter>()?.sharedMesh);
        Assert.NotNull(cube.transform.Find("CubeMesh")?.GetComponent<BoxCollider>());

        GameObject physics = LoadAsset<GameObject>("Assets/Prefabs/Lessons/PhysicsLesson.prefab");
        Assert.NotNull(physics.GetComponent<PhysicsLessonController>());
        Assert.NotNull(physics.GetComponent<ARObjectManipulator>());
        Assert.NotNull(physics.transform.Find("PhysicsBall")?.GetComponent<MeshFilter>()?.sharedMesh);
        Assert.NotNull(physics.transform.Find("PhysicsBall")?.GetComponent<TrailRenderer>());
        Assert.NotNull(physics.transform.Find("PhysicsTrack")?.GetComponent<MeshFilter>()?.sharedMesh);
        Assert.NotNull(physics.transform.Find("PhysicsTrack")?.GetComponent<MeshRenderer>()?.sharedMaterial);
    }

    private static T LoadAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Assert.NotNull(asset, $"Missing asset at {path}");
        return asset;
    }
}
