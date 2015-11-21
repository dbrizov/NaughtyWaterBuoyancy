using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    public class FluidVolume : MonoBehaviour
    {
        private const float CLOSE_VERTEX_THRESHOLD = 2f;
        
        private Mesh mesh;

        protected virtual void Awake()
        {
            this.mesh = this.GetComponent<MeshFilter>().mesh;
        }

        private bool IsPointUnderwater(Vector3 point, IEnumerable<Vector3> fluidVerticesInWorldSpace)
        {
            var closeVerts = fluidVerticesInWorldSpace.Where(
                    v => Vector3.Distance(new Vector3(v.x, 0f, v.z), new Vector3(point.x, 0f, point.z)) <= CLOSE_VERTEX_THRESHOLD);

            if (closeVerts.Any(v => point.y < v.y))
            {
                return true;
            }

            return false;
        }

        //protected virtual void OnTriggerEnter(Collider other)
        //{
        //    if (this.IsValidGameObject(other.gameObject))
        //    {
        //        var affectedGO = new AffectedGameObject(other.gameObject);
        //        this.affectedGameObjectById.Add(other.GetInstanceID(), affectedGO);
        //    }
        //}

        //protected virtual void OnTriggerExit(Collider other)
        //{
        //    if (this.IsValidGameObject(other.gameObject))
        //    {
        //        this.affectedGameObjectById.Remove(other.GetInstanceID());
        //    }
        //}

        //private bool IsValidGameObject(GameObject gameObject)
        //{
        //    if (gameObject.GetComponent<Rigidbody>() != null &&
        //        gameObject.GetComponent<BoxCollider>() != null &&
        //        gameObject.GetComponent<MeshFilter>() != null)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //private float GetMaxDimention(AffectedGameObject affectedGO)
        //{
        //    var bounds = Vector3.Scale(affectedGO.BoxCollider.size, affectedGO.GameObject.transform.localScale);
        //    return Mathf.Max(bounds.x, bounds.y, bounds.z);
        //}

        //private Vector3[] GetBoxColliderVerticesInWorldSpace(BoxCollider boxCollider)
        //{
        //    Vector3 halfSize = boxCollider.size / 2f;
        //    Matrix4x4 l2w = boxCollider.gameObject.transform.localToWorldMatrix;

        //    Vector3[] vertices = new Vector3[]
        //        {
        //            l2w.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, -halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, -halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, -halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, halfSize.z)),
        //            l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, -halfSize.z)),
        //        };

        //    return vertices;
        //}

        //private List<Vector3> GetUnderwaterVerticesInWorldSpace(AffectedGameObject affectedGO, IEnumerable<Vector3> fluidVerticesInWorldSpace)
        //{
        //    List<Vector3> underwaterVertices = new List<Vector3>();
        //    var verticesInWorldSpace = this.GetBoxColliderVerticesInWorldSpace(affectedGO.BoxCollider);

        //    foreach (var vert in verticesInWorldSpace)
        //    {
        //        if (this.IsPointUnderwater(vert, fluidVerticesInWorldSpace))
        //        {
        //            underwaterVertices.Add(vert);
        //        }
        //    }

        //    return underwaterVertices;
        //}

        //private Vector3 GetMidPoint(IEnumerable<Vector3> points)
        //{
        //    int count = 0;
        //    Vector3 sum = Vector3.zero;

        //    foreach (var point in points)
        //    {
        //        sum += point;
        //        count++;
        //    }

        //    if (count == 0)
        //    {
        //        return Vector3.zero;
        //    }

        //    return sum / count;
        //}
    }
}
