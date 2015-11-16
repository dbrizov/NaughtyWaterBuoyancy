using UnityEngine;
namespace FluidBuoyancy
{
    public class WaterWaves : MonoBehaviour
    {
        [SerializeField]
        private float speed = 1f;

        [SerializeField]
        private float height = 0.2f;

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] vertices;

        protected virtual void Awake()
        {
            this.mesh = this.GetComponent<MeshFilter>().mesh;
            this.baseVertices = this.mesh.vertices;
            this.vertices = new Vector3[this.baseVertices.Length];
        }

        protected virtual void Update()
        {
            for (var i = 0; i < this.vertices.Length; i++)
            {
                var vertex = this.baseVertices[i];
                vertex.y += Mathf.Sin(Time.time * this.speed + this.baseVertices[i].x + this.baseVertices[i].y + this.baseVertices[i].z) * this.height;
                this.vertices[i] = vertex;
            }

            this.mesh.vertices = this.vertices;
            this.mesh.RecalculateNormals();
        }
    }
}
