using UnityEngine;

namespace WaterBuoyancy
{
    public static class StringUtils
    {
        public static string Vector3ToString(Vector3 vector)
        {
            return string.Format("({0:f3}, {1:f3}, {2:f3})", vector.x, vector.y, vector.z);
        }
    }
}
