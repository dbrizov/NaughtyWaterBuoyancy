using UnityEngine;

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
}
