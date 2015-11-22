using UnityEngine;

namespace WaterBuoyancy
{
    public static class DebugUtils
    {
        public static void DrawPoint(Vector3 point)
        {
            DrawPoint(point, Color.white);
        }

        public static void DrawPoint(Vector3 point, Color color)
        {
            Debug.DrawLine(point - Vector3.right * 0.1f, point + Vector3.right * 0.1f, color);
            Debug.DrawLine(point - Vector3.up * 0.1f, point + Vector3.up * 0.1f, color);
            Debug.DrawLine(point - Vector3.forward * 0.1f, point + Vector3.forward * 0.1f, color);
        }

        public static void DrawPoint(float x, float y, float z)
        {
            DrawPoint(new Vector3(x, y, z));
        }

        public static void DrawPoint(float x, float y, float z, Color color)
        {
            DrawPoint(new Vector3(x, y, z), color);
        }
    }
}
