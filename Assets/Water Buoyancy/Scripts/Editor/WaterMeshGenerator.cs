using UnityEngine;

namespace WaterBuoyancy
{
    public class WaterMeshGenerator
    {
        private static int rows;
        private static int columns;
        private static float quadSegmentSize;

        public static Mesh GenerateMesh(int _rows, int _columns, float _quadSegmentSize)
        {
            if (_rows < 0f || _columns < 0 || _quadSegmentSize < 0f)
            {
                throw new System.ArgumentException("Invalid water mesh data");
            }

            rows = _rows + 1; // There are 2 rows between 3 points, so we need to add 1
            columns = _columns + 1; // Same here
            quadSegmentSize = _quadSegmentSize;

            var mesh = new Mesh();
            mesh.name = "Water Mesh";

            MeshData meshData = new MeshData();
            meshData.Vertices = new Vector3[rows * columns];
            meshData.Normals = new Vector3[rows * columns];
            meshData.UVs = new Vector2[rows * columns];
            meshData.TriangleIndices = new int[rows * columns * 6];

            int triangleIndex = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int index = GetIndex(r, c);

                    // Set vertices, normals and UVs
                    meshData.Vertices[index] = new Vector3(c * quadSegmentSize, 0f, r * quadSegmentSize);
                    meshData.Normals[index] = Vector3.up;
                    meshData.UVs[index] = new Vector2((float)c / columns, (float)r / rows);

                    // Set triangles
                    if (r < rows - 1 && c < columns - 1)
                    {
                        meshData.TriangleIndices[triangleIndex + 0] = GetIndex(r, c);
                        meshData.TriangleIndices[triangleIndex + 1] = GetIndex(r + 1, c);
                        meshData.TriangleIndices[triangleIndex + 2] = GetIndex(r, c + 1);

                        meshData.TriangleIndices[triangleIndex + 3] = GetIndex(r + 1, c);
                        meshData.TriangleIndices[triangleIndex + 4] = GetIndex(r + 1, c + 1);
                        meshData.TriangleIndices[triangleIndex + 5] = GetIndex(r, c + 1);

                        triangleIndex += 6;
                    }
                }
            }

            mesh.vertices = meshData.Vertices;
            mesh.normals = meshData.Normals;
            mesh.uv = meshData.UVs;
            mesh.triangles = meshData.TriangleIndices;

            return mesh;
        }

        private static int GetIndex(int row, int column)
        {
            return row * columns + column;
        }

        private static int GetRow(int vertexIndex)
        {
            return vertexIndex / columns;
        }

        private static int GetColumn(int vertexIndex)
        {
            return vertexIndex % columns;
        }

        private struct MeshData
        {
            public Vector3[] Vertices { get; set; }
            public Vector3[] Normals { get; set; }
            public Vector2[] UVs { get; set; }
            public int[] TriangleIndices { get; set; }
        }
    }
}
