using UnityEngine;

namespace WaterBuoyancy
{
    public static class ColliderUtils
    {
        public static bool IsPointInsideCollider(Vector3 point, Collider collider, ref Bounds colliderBounds)
        {
            float rayLength = colliderBounds.size.magnitude;
            Ray ray = new Ray(point, collider.transform.position - point);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength))
            {
                if (hit.collider == collider)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
