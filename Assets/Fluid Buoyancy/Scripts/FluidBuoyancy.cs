using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    public class FluidBuoyancy : MonoBehaviour
    {
        private const float CLOSE_VERTEX_RADIUS = 1.5f;

        [SerializeField]
        private float force = 15f;

        [SerializeField]
        [Tooltip("Stickiness")]
        private float viscosity = 2f;

        private Mesh mesh;
        private Dictionary<int, AffectedGameObject> affectedGameObjectById;

        protected virtual void Awake()
        {
            this.mesh = this.GetComponent<MeshFilter>().mesh;
            this.affectedGameObjectById = new Dictionary<int, AffectedGameObject>();
        }

        protected virtual void Update()
        {
            var force = new Vector3(0f, this.force, 0f);
            var fluidVerticesInWorldSpace = this.mesh.vertices.Select(v => this.transform.localToWorldMatrix.MultiplyPoint3x4(v));

            foreach (var affectedGOById in this.affectedGameObjectById)
            {
                List<Vector3> underwaterVertices = this.GetUnderwaterVerticesInWorldSpace(affectedGOById.Value, fluidVerticesInWorldSpace);
                if (underwaterVertices.Count > 0f)
                {
                    Vector3 midPoint = this.GetMidPoint(underwaterVertices);

                    Rigidbody rigidbody = affectedGOById.Value.Rigidbody;
                    rigidbody.AddForceAtPosition(force, midPoint);
                    rigidbody.AddForceAtPosition(rigidbody.velocity * -1f * this.viscosity, midPoint);
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
                var closeVerts = fluidVerticesInWorldSpace.Where(
                    v => Vector3.Distance(new Vector3(v.x, 0f, v.z), new Vector3(vert.x, 0f, vert.z)) <= CLOSE_VERTEX_RADIUS);

                if (closeVerts.Any(v => vert.y < v.y))
                {
                    underwaterVertices.Add(vert);
                }
            }

            foreach (var vert in underwaterVertices)
            {
                Debug.DrawLine(vert - Vector3.right * 0.05f, vert + Vector3.right * 0.05f, Color.red);
                Debug.DrawLine(vert - Vector3.up * 0.05f, vert + Vector3.up * 0.05f, Color.red);
            }

            return underwaterVertices;
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
