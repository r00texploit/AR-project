using UnityEngine;

namespace AREducation.Utils
{
    /// <summary>
    /// Generates procedural meshes for lesson objects.
    /// All geometry is AR-scaled: 1 lesson unit ≈ 10 cm in world space.
    /// </summary>
    public static class MeshGenerator
    {
        private const float ArScale = 0.1f;

        // Returns mesh + the three world-space vertex positions (scaled) for outline use.
        public static (Mesh mesh, Vector3 vA, Vector3 vB, Vector3 vC)
            GenerateTriangle(float sideA, float sideB, float sideC, float thickness = 0.04f)
        {
            // Clamp for numerical safety
            sideA = Mathf.Max(sideA, 0.01f);
            sideB = Mathf.Max(sideB, 0.01f);
            sideC = Mathf.Max(sideC, 0.01f);

            // Place A at origin, B along +X at distance sideC (AB = sideC).
            // Find C using cosine rule for angle at A:
            //   cos(A) = (b² + c² - a²) / (2bc)
            // where a = sideA (opposite A = BC), b = sideB (CA), c = sideC (AB).
            float cosA = (sideB * sideB + sideC * sideC - sideA * sideA)
                        / (2f * sideB * sideC);
            cosA = Mathf.Clamp(cosA, -0.9999f, 0.9999f);
            float sinA = Mathf.Sqrt(1f - cosA * cosA);

            Vector3 vA = Vector3.zero;
            Vector3 vB = new Vector3(sideC, 0f, 0f);
            Vector3 vC = new Vector3(sideB * cosA, 0f, sideB * sinA);

            // Centre at origin for stable rotation
            Vector3 centroid = (vA + vB + vC) / 3f;
            vA -= centroid; vB -= centroid; vC -= centroid;

            // Apply AR scale
            vA *= ArScale; vB *= ArScale; vC *= ArScale;

            // Extrude into a triangular prism for 3D appearance
            Vector3 up = Vector3.up * (thickness * ArScale * 10f);

            Vector3 vA2 = vA + up;
            Vector3 vB2 = vB + up;
            Vector3 vC2 = vC + up;

            // Vertices: bottom face (0-2), top face (3-5), and 3 side quads (6-17)
            Vector3[] vertices = new Vector3[]
            {
                // Bottom
                vA, vB, vC,
                // Top
                vA2, vB2, vC2,
                // Side AB: vA, vB, vB2, vA2
                vA, vB, vB2, vA2,
                // Side BC: vB, vC, vC2, vB2
                vB, vC, vC2, vB2,
                // Side CA: vC, vA, vA2, vC2
                vC, vA, vA2, vC2,
            };

            Vector3[] normals = new Vector3[]
            {
                Vector3.down, Vector3.down, Vector3.down,
                Vector3.up, Vector3.up, Vector3.up,
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
            };

            int[] triangles = new int[]
            {
                // Bottom (reversed winding for downward normal)
                0, 2, 1,
                // Top
                3, 4, 5,
                // Side AB
                6, 7, 8,  6, 8, 9,
                // Side BC
                10, 11, 12,  10, 12, 13,
                // Side CA
                14, 15, 16,  14, 16, 17,
            };

            Mesh mesh = new Mesh { name = "ProceduralTriangle" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return (mesh, vA, vB, vC);
        }

        public static Mesh GenerateCube(float size)
        {
            float h = size * ArScale / 2f;

            // 6 faces × 4 verts = 24 verts (unshared for correct per-face normals)
            Vector3[] vertices = new Vector3[24];
            Vector3[] normals  = new Vector3[24];
            Vector2[] uvs      = new Vector2[24];
            int[]     tris     = new int[36];

            // Each face: (normal, [4 corner positions])
            (Vector3 n, Vector3[] c)[] faces =
            {
                (Vector3.forward,  new[]{ new Vector3(-h,-h, h), new Vector3( h,-h, h), new Vector3( h, h, h), new Vector3(-h, h, h) }),
                (Vector3.back,     new[]{ new Vector3( h,-h,-h), new Vector3(-h,-h,-h), new Vector3(-h, h,-h), new Vector3( h, h,-h) }),
                (Vector3.left,     new[]{ new Vector3(-h,-h,-h), new Vector3(-h,-h, h), new Vector3(-h, h, h), new Vector3(-h, h,-h) }),
                (Vector3.right,    new[]{ new Vector3( h,-h, h), new Vector3( h,-h,-h), new Vector3( h, h,-h), new Vector3( h, h, h) }),
                (Vector3.up,       new[]{ new Vector3(-h, h, h), new Vector3( h, h, h), new Vector3( h, h,-h), new Vector3(-h, h,-h) }),
                (Vector3.down,     new[]{ new Vector3(-h,-h,-h), new Vector3( h,-h,-h), new Vector3( h,-h, h), new Vector3(-h,-h, h) }),
            };

            Vector2[] faceUVs = { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };

            for (int f = 0; f < 6; f++)
            {
                int vBase = f * 4;
                int tBase = f * 6;
                for (int v = 0; v < 4; v++)
                {
                    vertices[vBase + v] = faces[f].c[v];
                    normals [vBase + v] = faces[f].n;
                    uvs     [vBase + v] = faceUVs[v];
                }
                tris[tBase + 0] = vBase + 0; tris[tBase + 1] = vBase + 1; tris[tBase + 2] = vBase + 2;
                tris[tBase + 3] = vBase + 0; tris[tBase + 4] = vBase + 2; tris[tBase + 5] = vBase + 3;
            }

            Mesh mesh = new Mesh { name = "ProceduralCube" };
            mesh.vertices  = vertices;
            mesh.triangles = tris;
            mesh.normals   = normals;
            mesh.uv        = uvs;
            mesh.RecalculateBounds();
            return mesh;
        }

        // Flat double-sided circle for a "ball shadow" indicator
        public static Mesh GenerateDisk(float radius, int segments = 32)
        {
            radius *= ArScale;
            Vector3[] verts = new Vector3[segments + 1];
            int[]     tris  = new int[segments * 3 * 2];
            verts[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments + 1;
                // Front face
                tris[i * 3]     = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = next;
                // Back face
                tris[segments * 3 + i * 3]     = 0;
                tris[segments * 3 + i * 3 + 1] = next;
                tris[segments * 3 + i * 3 + 2] = i + 1;
            }
            Mesh mesh = new Mesh { name = "ProceduralDisk" };
            mesh.vertices  = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
