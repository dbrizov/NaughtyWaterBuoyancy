using System.Collections.Generic;
using UnityEngine;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class BouyancyForce : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float normalizedVoxelSize = 0.25f;

        [SerializeField]
        private float dragInWater = 1f;

        [SerializeField]
        private float angularDragInWater = 1f;

        private bool isInWater;
        private new Collider collider;
        private Vector3 voxelSize;
        private Vector3[] voxels;

        protected virtual void Awake()
        {
            this.collider = this.GetComponent<Collider>();

            var bounds = this.collider.bounds;
            this.voxelSize.x = (this.normalizedVoxelSize * bounds.size.x); // / this.transform.localScale.x;
            this.voxelSize.y = (this.normalizedVoxelSize * bounds.size.y); // / this.transform.localScale.y;
            this.voxelSize.z = (this.normalizedVoxelSize * bounds.size.z); // / this.transform.localScale.z;
        }

        protected virtual void FixedUpdate()
        {
            if (this.isInWater)
            {
                // Apply bouyancy force
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(WaterVolume.TAG))
            {
                this.isInWater = true;
                if (this.voxels == null)
                {
                    this.voxels = this.GetVoxels();
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(WaterVolume.TAG))
            {
                this.isInWater = false;
            }
        }

        private Vector3[] GetVoxels()
        {
            var colliderBounds = this.collider.bounds;
            int xRes = Mathf.RoundToInt(colliderBounds.size.x / this.voxelSize.x);
            int yRes = Mathf.RoundToInt(colliderBounds.size.y / this.voxelSize.y);
            int zRes = Mathf.RoundToInt(colliderBounds.size.z / this.voxelSize.z);
            List<Vector3> voxels = new List<Vector3>(xRes * yRes * zRes);

            for (int ix = 0; ix < xRes; ix++)
            {
                for (int iy = 0; iy < yRes; iy++)
                {
                    for (int iz = 0; iz < zRes; iz++)
                    {
                        float vX = colliderBounds.min.x + this.voxelSize.x * (0.5f + ix);
                        float vY = colliderBounds.min.y + this.voxelSize.y * (0.5f + iy);
                        float vZ = colliderBounds.min.z + this.voxelSize.z * (0.5f + iz);

                        Vector3 point = new Vector3(vX, vY, vZ);
                        if (ColliderUtils.IsPointInsideCollider(point, this.collider, ref colliderBounds))
                        {
                            voxels.Add(this.transform.InverseTransformPoint(point));
                        }
                    }
                }
            }

            return voxels.ToArray();
        }

        protected virtual void OnDrawGizmos()
        {
            if (this.voxels != null)
            {
                for (int i = 0; i < this.voxels.Length; i++)
                {
                    Gizmos.color = Color.magenta - new Color(0f, 0f, 0f, 0.75f);
                    Gizmos.DrawCube(this.transform.TransformPoint(this.voxels[i]), this.voxelSize * 0.8f);
                }
            }
        }
    }
}
