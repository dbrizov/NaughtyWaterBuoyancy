using UnityEngine;
using System.Collections.Generic;
using FluidBuoyancy.Collections;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    public class WaterVolume : MonoBehaviour
    {
        public const string TAG = "Water Volume";

        [SerializeField]
        private Transform trans;

        private Mesh mesh;
        private Vector3[] meshVertices;
        private Vector3[] meshWorldPoints;

        protected virtual void Awake()
        {
            this.mesh = this.GetComponent<MeshFilter>().mesh;
        }

        protected virtual void Update()
        {
            this.CacheMeshVertices();

            var points = this.GetClosestPointsOnWaterSurface(trans.position, 3);
            for (int i = 0; i < points.Length; i++)
            {
                DebugUtils.DrawPoint(points[i], Color.red);
            }

            if (this.IsPointUnderWater(trans.position))
            {
                trans.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            }
            else
            {
                trans.GetComponent<Renderer>().sharedMaterial.color = Color.green;
            }
        }

        public Vector3[] GetClosestPointsOnWaterSurface(Vector3 point, int pointsCount)
        {
            MinHeap<Vector3> closestPoints = new MinHeap<Vector3>(new Vector3HorizontalDistanceComparer(point));
            for (int i = 0; i < this.meshWorldPoints.Length; i++)
            {
                closestPoints.Add(this.meshWorldPoints[i]);
            }

            Vector3[] result = new Vector3[pointsCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = closestPoints.Remove();
            }

            return result;
        }

        private bool IsPointUnderWater(Vector3 point)
        {
            return this.GetWaterLevel(point) - point.y > 0f;
        }

        public float GetWaterLevel(Vector3 point)
        {
            Vector3[] meshPoligon = this.GetClosestPointsOnWaterSurface(point, 3);
            Vector3 planeV1 = meshPoligon[1] - meshPoligon[0];
            Vector3 planeV2 = meshPoligon[2] - meshPoligon[0];
            Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
            if (planeNormal.y < 0f)
            {
                planeNormal *= -1f;
            }

            float yOnWaterSurface = (-(point.x * planeNormal.x) - (point.z * planeNormal.z) + Vector3.Dot(meshPoligon[0], planeNormal)) / planeNormal.y;
            //Vector3 pointOnWaterSurface = new Vector3(point.x, yOnWaterSurface, point.z);
            //DebugUtils.DrawPoint(pointOnWaterSurface, Color.magenta);
            //Debug.DrawLine(pointOnWaterSurface, pointOnWaterSurface + planeNormal, Color.blue);
            //Debug.DrawLine(pointOnWaterSurface, meshPoligon[0], Color.green);
            //Debug.DrawLine(pointOnWaterSurface, meshPoligon[1], Color.green);
            //Debug.DrawLine(pointOnWaterSurface, meshPoligon[2], Color.green);

            return yOnWaterSurface;
        }

        private void CacheMeshVertices()
        {
            this.meshVertices = this.mesh.vertices;
            this.meshWorldPoints = this.ConvertPointsToWorldSpace(meshVertices);
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
