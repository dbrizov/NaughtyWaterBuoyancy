using UnityEngine;
using System.Collections.Generic;
using WaterBuoyancy.Collections;

namespace WaterBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class WaterVolume : MonoBehaviour
    {
        public const string TAG = "Water Volume";

        [SerializeField]
        private float density = 1f;

        [SerializeField]
        private int rows = 10;

        [SerializeField]
        private int columns = 10;

        [SerializeField]
        private float quadSegmentSize = 1f;

        //[SerializeField]
        //private Transform debugTrans; // Only for debugging

        private Mesh mesh;
        private Vector3[] meshLocalVertices;
        private Vector3[] meshWorldVertices;

        public float Density
        {
            get
            {
                return this.density;
            }
        }

        public int Rows
        {
            get
            {
                return this.rows;
            }
        }

        public int Columns
        {
            get
            {
                return this.columns;
            }
        }

        public float QuadSegmentSize
        {
            get
            {
                return this.quadSegmentSize;
            }
        }

        public Mesh Mesh
        {
            get
            {
                if (this.mesh == null)
                {
                    this.mesh = this.GetComponent<MeshFilter>().mesh;
                }

                return this.mesh;
            }
        }

        protected virtual void Awake()
        {
            this.CacheMeshVertices();
        }

        protected virtual void Update()
        {
            this.CacheMeshVertices();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = this.transform.localToWorldMatrix;

            Gizmos.DrawWireCube(this.GetComponent<BoxCollider>().center, this.GetComponent<BoxCollider>().size);
        }

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan - new Color(0f, 0f, 0f, 0.75f);
                Gizmos.matrix = this.transform.localToWorldMatrix;

                Gizmos.DrawCube(this.GetComponent<BoxCollider>().center - Vector3.up * 0.01f, this.GetComponent<BoxCollider>().size);

                Gizmos.color = Color.cyan - new Color(0f, 0f, 0f, 0.5f);
                Gizmos.DrawWireCube(this.GetComponent<BoxCollider>().center, this.GetComponent<BoxCollider>().size);

                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                // Draw sufrace normal
                //var vertices = this.meshWorldVertices;
                //var triangles = this.Mesh.triangles;
                //for (int i = 0; i < triangles.Length; i += 3)
                //{
                //    Gizmos.color = Color.white;
                //    Gizmos.DrawLine(vertices[triangles[i + 0]], vertices[triangles[i + 1]]);
                //    Gizmos.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                //    Gizmos.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i + 0]]);

                //    Vector3 center = MathfUtils.GetAveratePoint(vertices[triangles[i + 0]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                //    Vector3 normal = this.GetSurfaceNormal(center);

                //    Gizmos.color = Color.green;
                //    Gizmos.DrawLine(center, center + normal);
                //}

                // Draw mesh vertices
                //if (this.meshWorldVertices != null)
                //{
                //    for (int i = 0; i < this.meshWorldVertices.Length; i++)
                //    {
                //        DebugUtils.DrawPoint(this.meshWorldVertices[i], Color.red);
                //    }
                //}

                // Test GetSurroundingTrianglePolygon(Vector3 worldPoint);
                //if (debugTrans != null)
                //{
                //    Gizmos.color = Color.blue;
                //    Gizmos.DrawSphere(debugTrans.position, 0.1f);

                //    var point = debugTrans.position;
                //    var triangle = this.GetSurroundingTrianglePolygon(point);
                //    if (triangle != null)
                //    {
                //        Gizmos.color = Color.red;

                //        Gizmos.DrawLine(triangle[0], triangle[1]);
                //        Gizmos.DrawLine(triangle[1], triangle[2]);
                //        Gizmos.DrawLine(triangle[2], triangle[0]);
                //    }
                //}
            }
        }

        public Vector3[] GetSurroundingTrianglePolygon(Vector3 worldPoint)
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(worldPoint);
            int x = Mathf.CeilToInt(localPoint.x / this.QuadSegmentSize);
            int z = Mathf.CeilToInt(localPoint.z / this.QuadSegmentSize);
            if (x <= 0 || z <= 0 || x >= (this.Columns + 1) || z >= (this.Rows + 1))
            {
                return null;
            }

            Vector3[] trianglePolygon = new Vector3[3];
            if ((worldPoint - this.meshWorldVertices[this.GetIndex(z, x)]).sqrMagnitude <
                ((worldPoint - this.meshWorldVertices[this.GetIndex(z - 1, x - 1)]).sqrMagnitude))
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z, x)];
            }
            else
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z - 1, x - 1)];
            }

            trianglePolygon[1] = this.meshWorldVertices[this.GetIndex(z - 1, x)];
            trianglePolygon[2] = this.meshWorldVertices[this.GetIndex(z, x - 1)];

            return trianglePolygon;
        }

        public Vector3[] GetClosestPointsOnWaterSurface(Vector3 worldPoint, int pointsCount)
        {
            MinHeap<Vector3> allPoints = new MinHeap<Vector3>(new Vector3HorizontalDistanceComparer(worldPoint));
            for (int i = 0; i < this.meshWorldVertices.Length; i++)
            {
                allPoints.Add(this.meshWorldVertices[i]);
            }

            Vector3[] closestPoints = new Vector3[pointsCount];
            for (int i = 0; i < closestPoints.Length; i++)
            {
                closestPoints[i] = allPoints.Remove();
            }

            return closestPoints;
        }

        public Vector3 GetSurfaceNormal(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                return planeNormal;
            }

            return this.transform.up;
        }

        public float GetWaterLevel(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                // Plane equation
                float yOnWaterSurface = (-(worldPoint.x * planeNormal.x) - (worldPoint.z * planeNormal.z) + Vector3.Dot(meshPolygon[0], planeNormal)) / planeNormal.y;
                //Vector3 pointOnWaterSurface = new Vector3(point.x, yOnWaterSurface, point.z);
                //DebugUtils.DrawPoint(pointOnWaterSurface, Color.magenta);

                return yOnWaterSurface;
            }

            return this.transform.position.y;
        }

        public bool IsPointUnderWater(Vector3 worldPoint)
        {
            return this.GetWaterLevel(worldPoint) - worldPoint.y > 0f;
        }

        private int GetIndex(int row, int column)
        {
            return row * (this.Columns + 1) + column;
        }

        private void CacheMeshVertices()
        {
            this.meshLocalVertices = this.Mesh.vertices;
            this.meshWorldVertices = this.ConvertPointsToWorldSpace(meshLocalVertices);
        }

        private Vector3[] ConvertPointsToWorldSpace(Vector3[] localPoints)
        {
            Vector3[] worldPoints = new Vector3[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
            {
                worldPoints[i] = this.transform.TransformPoint(localPoints[i]);
            }

            return worldPoints;
        }

        private class Vector3HorizontalDistanceComparer : IComparer<Vector3>
        {
            private Vector3 distanceToVector;

            public Vector3HorizontalDistanceComparer(Vector3 distanceTo)
            {
                this.distanceToVector = distanceTo;
            }

            public int Compare(Vector3 v1, Vector3 v2)
            {
                v1.y = 0;
                v2.y = 0;
                float v1Distance = (v1 - distanceToVector).sqrMagnitude;
                float v2Distance = (v2 - distanceToVector).sqrMagnitude;

                if (v1Distance < v2Distance)
                {
                    return -1;
                }
                else if (v1Distance > v2Distance)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
