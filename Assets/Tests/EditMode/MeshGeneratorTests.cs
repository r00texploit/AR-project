using AREducation.Utils;
using NUnit.Framework;
using UnityEngine;

public class MeshGeneratorTests
{
    [Test]
    public void CubeHasSixSubmeshesAndBounds()
    {
        Mesh cube = MeshGenerator.GenerateCube(1f);
        Assert.AreEqual(6, cube.subMeshCount);
        Assert.Greater(cube.vertexCount, 0);
        Assert.Greater(cube.bounds.size.x, 0f);
        Assert.Greater(cube.bounds.size.y, 0f);
        Assert.Greater(cube.bounds.size.z, 0f);
    }

    [Test]
    public void TriangleProducesPrismMeshAndOutlinePoints()
    {
        var (mesh, a, b, c) = MeshGenerator.GenerateTriangle(3f, 4f, 5f);
        Assert.Greater(mesh.vertexCount, 0);
        Assert.Greater(mesh.triangles.Length, 0);
        Assert.AreNotEqual(a, b);
        Assert.AreNotEqual(b, c);
        Assert.AreNotEqual(c, a);
    }
}
