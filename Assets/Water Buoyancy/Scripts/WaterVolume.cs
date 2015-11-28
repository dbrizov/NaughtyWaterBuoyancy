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

        [SerializeField]
        private Transform debugTrans; // Only for debugging

        private Mesh mesh;
        private Vector3[] meshLocalVertices;
        private Vector3[] meshWorldVertices;
        private List<Vector3[]> meshTrianglesInWorldSpace;
        private Vector3[] lastCachedTrianglePolygon;

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
            this.CacheMeshTrianglesInWorldSpace();
        }

        protected virtual void Update()
        {
            this.CacheMeshVertices();
            this.CacheMeshTrianglesInWorldSpace();
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
            }

            if (this.meshTrianglesInWorldSpace == null)
            {
                return;
            }

            Gizmos.matrix = Matrix4x4.identity;
            if (debugTrans != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(debugTrans.position, 0.1f);

                for (int i = 0; i < this.meshTrianglesInWorldSpace.Count; i++)
                {
                    if (MathfUtils.IsPointInTriangle(debugTrans.position, this.meshTrianglesInWorldSpace[i], false, true, false))
                    {
                        Gizmos.color = Color.green;

                        Gizmos.DrawLine(this.meshTrianglesInWorldSpace[i][0], this.meshTrianglesInWorldSpace[i][1]);
                        Gizmos.DrawLine(this.meshTrianglesInWorldSpace[i][1], this.meshTrianglesInWorldSpace[i][2]);
                        Gizmos.DrawLine(this.meshTrianglesInWorldSpace[i][2], this.meshTrianglesInWorldSpace[i][0]);
                    }
                }
            }

            //if (this.meshWorldVertices != null)
            //{
            //    for (int i = 0; i < this.meshWorldVertices.Length; i++)
            //    {
            //        DebugUtils.DrawPoint(this.meshWorldVertices[i], Color.red);
            //    }
            //}
        }

        public Vector3[] GetSurroundingTrianglePolygon(Vector3 point)
        {
            if (this.lastCachedTrianglePolygon != null &&
                MathfUtils.IsPointInTriangle(point, this.lastCachedTrianglePolygon, false, true, false))
            {
                return this.lastCachedTrianglePolygon;
            }

            for (int i = 0; i < this.meshTrianglesInWorldSpace.Count; i++)
            {
                if (MathfUtils.IsPointInTriangle(point, this.meshTrianglesInWorldSpace[i], false, true, false))
                {
                    this.lastCachedTrianglePolygon = this.meshTrianglesInWorldSpace[i];
                    return this.lastCachedTrianglePolygon;
                }
            }

            return null;
        }

        public Vector3[] GetClosestPointsOnWaterSurface(Vector3 point, int pointsCount)
        {
            MinHeap<Vector3> allPoints = new MinHeap<Vector3>(new Vector3HorizontalDistanceComparer(point));
            for (int i = 0; i < this.meshWorldVertices.Length; i++)
            {
                allPoints.Add(this.meshWorldVertices[i]);
            }

            Vector3[] closest = new Vector3[pointsCount];
            for (int i = 0; i < closest.Length; i++)
            {
                closest[i] = allPoints.Remove();
            }

            return closest;
        }

        public Vector3 GetSurfaceNormal(Vector3 point)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(point);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;

                return planeNormal;
            }

            return this.transform.up;
        }

        public float GetWaterLevel(Vector3 point)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(point);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                float yOnWaterSurface = (-(point.x * planeNormal.x) - (point.z * planeNormal.z) + Vector3.Dot(meshPolygon[0], planeNormal)) / planeNormal.y;
                //Vector3 pointOnWaterSurface = new Vector3(point.x, yOnWaterSurface, point.z);
                //DebugUtils.DrawPoint(pointOnWaterSurface, Color.magenta);

                return yOnWaterSurface;
            }

            return this.transform.position.y;
        }

        public bool IsPointUnderWater(Vector3 point)
        {
            return this.GetWaterLevel(point) - point.y > 0f;
        }

        private void CacheMeshVertices()
        {
            this.meshLocalVertices = this.Mesh.vertices;
            this.meshWorldVertices = this.ConvertPointsToWorldSpace(meshLocalVertices);
        }

        private void CacheMeshTrianglesInWorldSpace()
        {
            int[] triangles = this.Mesh.triangles;
            if (this.meshTrianglesInWorldSpace == null)
            {
                this.meshTrianglesInWorldSpace = new List<Vector3[]>(triangles.Length / 3);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    this.meshTrianglesInWorldSpace.Add(new Vector3[3]);
                }
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                this.meshTrianglesInWorldSpace[i / 3][0] = this.meshWorldVertices[triangles[i]];
                this.meshTrianglesInWorldSpace[i / 3][1] = this.meshWorldVertices[triangles[i + 1]];
                this.meshTrianglesInWorldSpace[i / 3][2] = this.meshWorldVertices[triangles[i + 2]];
            }
        }

        private Vector3[] ConvertPointsToWorldSpace(Vector3[] points)
        {
            Vector3[] worldPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[i] = this.transform.TransformPoint(points[i]);
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
