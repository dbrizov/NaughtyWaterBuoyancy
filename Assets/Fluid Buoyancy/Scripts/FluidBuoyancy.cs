using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    public class FluidBuoyancy : MonoBehaviour
    {
        private const float CLOSE_VERTEX_THRESHOLD = 2f;
        private const float SURFACE_RAYCAST_STEP = 0.5f;

        [SerializeField]
        private float force = 15f;

        [SerializeField]
        [Tooltip("Stickiness")]
        private float viscosity = 2f;

        [SerializeField]
        private LayerMask ignoreLayer = 0;
        
        private Mesh mesh;
        private Dictionary<int, AffectedGameObject> affectedGameObjectById;

        protected virtual void Awake()
        {
            this.mesh = this.GetComponent<MeshFilter>().mesh;
            this.affectedGameObjectById = new Dictionary<int, AffectedGameObject>();
        }

        protected virtual void FixedUpdate()
        {
            var force = new Vector3(0f, this.force, 0f);
            var fluidVerticesInWorldSpace = this.mesh.vertices.Select(v => this.transform.localToWorldMatrix.MultiplyPoint3x4(v));

            foreach (var affectedGOById in this.affectedGameObjectById)
            {
                Vector3 center = affectedGOById.Value.GameObject.transform.localToWorldMatrix.MultiplyPoint3x4(affectedGOById.Value.BoxCollider.center);
                if (this.IsPointUnderwater(center, fluidVerticesInWorldSpace))
                {
                    List<Vector3> underwaterVertices = this.GetUnderwaterVerticesInWorldSpace(affectedGOById.Value, fluidVerticesInWorldSpace);
                    underwaterVertices.Add(center);

                    foreach (var vert in underwaterVertices)
                    {
                        Debug.DrawLine(vert - Vector3.right * 0.05f, vert + Vector3.right * 0.05f, Color.red);
                        Debug.DrawLine(vert - Vector3.up * 0.05f, vert + Vector3.up * 0.05f, Color.red);
                    }

                    Vector3 midPoint = this.GetMidPoint(underwaterVertices);
                    Debug.DrawLine(midPoint, midPoint + Vector3.up * 3f, Color.blue);

                    Rigidbody rigidbody = affectedGOById.Value.Rigidbody;
                    rigidbody.AddForceAtPosition(force, midPoint, ForceMode.Acceleration);
                    rigidbody.AddForceAtPosition(rigidbody.velocity * -1f * this.viscosity, midPoint, ForceMode.Acceleration);

                    // Add surface drag
                    float maxDimention = this.GetMaxDimention(affectedGOById.Value);
                    Vector3 frontPoint = affectedGOById.Value.GameObject.transform.position + (rigidbody.velocity.normalized * maxDimention);
                    Vector3 planeVector1 = Vector3.Cross(rigidbody.velocity, Vector3.right).normalized;
                    Vector3 planeVector2 = Vector3.Cross(rigidbody.velocity, planeVector1).normalized;

                    float halfMaxDimention = maxDimention / 2f;
                    for (float x = -halfMaxDimention; x <= halfMaxDimention; x += SURFACE_RAYCAST_STEP)
                    {
                        for (float y = -halfMaxDimention; y <= halfMaxDimention; y += SURFACE_RAYCAST_STEP)
                        {
                            Vector3 start = frontPoint + (planeVector1 * x) + (planeVector2 * y);
                            Ray ray = new Ray(start, -rigidbody.velocity);
                            RaycastHit hit;
                            if (Physics.Raycast(ray, out hit, maxDimention * 2f, ~this.ignoreLayer))
                            {
                                if (hit.transform.gameObject == affectedGOById.Value.GameObject)
                                {
                                    Debug.DrawLine(ray.origin, hit.point, Color.magenta);
                                    rigidbody.AddForceAtPosition(rigidbody.velocity * -0.05f, hit.point, ForceMode.Acceleration);
                                }
                            }
                        }
                    }

                    //Debug.DrawLine(center + rigidbody.velocity.normalized + planeVector1, center + rigidbody.velocity.normalized - planeVector1, Color.magenta);
                    //Debug.DrawLine(center + rigidbody.velocity.normalized + planeVector2, center + rigidbody.velocity.normalized - planeVector2, Color.magenta);
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (this.IsValidGameObject(other.gameObject))
            {
                var affectedGO = new AffectedGameObject(other.gameObject);
                this.affectedGameObjectById.Add(other.GetInstanceID(), affectedGO);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (this.IsValidGameObject(other.gameObject))
            {
                this.affectedGameObjectById.Remove(other.GetInstanceID());
            }
        }

        private bool IsValidGameObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<Rigidbody>() != null &&
                gameObject.GetComponent<BoxCollider>() != null &&
                gameObject.GetComponent<MeshFilter>() != null)
            {
                return true;
            }

            return false;
        }

        private float GetMaxDimention(AffectedGameObject affectedGO)
        {
            var bounds = Vector3.Scale(affectedGO.BoxCollider.size, affectedGO.GameObject.transform.localScale);
            return Mathf.Max(bounds.x, bounds.y, bounds.z);
        }

        private Vector3[] GetBoxColliderVerticesInWorldSpace(BoxCollider boxCollider)
        {
            Vector3 halfSize = boxCollider.size / 2f;
            Matrix4x4 l2w = boxCollider.gameObject.transform.localToWorldMatrix;

            Vector3[] vertices = new Vector3[]
                {
                    l2w.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(halfSize.x, halfSize.y, -halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(halfSize.x, -halfSize.y, -halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, halfSize.y, -halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, halfSize.z)),
                    l2w.MultiplyPoint3x4(new Vector3(-halfSize.x, -halfSize.y, -halfSize.z)),
                };

            return vertices;
        }

        private List<Vector3> GetUnderwaterVerticesInWorldSpace(AffectedGameObject affectedGO, IEnumerable<Vector3> fluidVerticesInWorldSpace)
        {
            List<Vector3> underwaterVertices = new List<Vector3>();
            var verticesInWorldSpace = this.GetBoxColliderVerticesInWorldSpace(affectedGO.BoxCollider);

            foreach (var vert in verticesInWorldSpace)
            {
                if (this.IsPointUnderwater(vert, fluidVerticesInWorldSpace))
                {
                    underwaterVertices.Add(vert);
                }
            }

            return underwaterVertices;
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

        private Vector3 GetMidPoint(IEnumerable<Vector3> points)
        {
            int count = 0;
            Vector3 sum = Vector3.zero;

            foreach (var point in points)
            {
                sum += point;
                count++;
            }

            if (count == 0)
            {
                return Vector3.zero;
            }

            return sum / count;
        }
    }
}
