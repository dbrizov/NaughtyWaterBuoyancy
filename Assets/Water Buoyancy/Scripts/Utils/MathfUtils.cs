using UnityEngine;

namespace FluidBuoyancy
{
    public static class MathfUtils
    {
        public static float CalculateArea_Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float a = (p1 - p2).magnitude;
            float b = (p1 - p3).magnitude;
            float c = (p2 - p3).magnitude;
            float p = (a + b + c) / 2f; // The half perimeter

            return Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        public static float CalculateVolume_Mesh(Mesh mesh, Transform trans)
        {
            float volume = 0f;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];

                volume += CalculateVolume_Tetrahedron(p1, p2, p3, Vector3.zero);
            }

            return Mathf.Abs(volume) * trans.localScale.x * trans.localScale.y * trans.localScale.z;
        }

        public static float CalculateVolume_Tetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3 a = p1 - p2;
            Vector3 b = p1 - p3;
            Vector3 c = p1 - p4;

            return (Vector3.Dot(a, Vector3.Cross(b, c))) / 6f;

            ////float v321 = p3.x * p2.y * p1.z;
            ////float v231 = p2.x * p3.y * p1.z;
            ////float v312 = p3.x * p1.y * p2.z;
            ////float v132 = p1.x * p3.y * p2.z;
            ////float v213 = p2.x * p1.y * p3.z;
            ////float v123 = p1.x * p2.y * p3.z;

            ////return (1f / 6f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }
    }
}
