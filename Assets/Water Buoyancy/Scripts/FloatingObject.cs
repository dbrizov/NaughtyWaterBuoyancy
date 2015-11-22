using System.Collections.Generic;
using UnityEngine;

namespace WaterBuoyancy
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter))]
    public class FloatingObject : MonoBehaviour
    {
        [SerializeField]
        private bool calculateDensity = false;

        [SerializeField]
        private float density = 0.75f;

        [SerializeField]
        [Range(0f, 1f)]
        private float normalizedVoxelSize = 0.25f;

        [SerializeField]
        private float dragInWater = 1f;

        [SerializeField]
        private float angularDragInWater = 1f;

        private WaterVolume water;
        private new Collider collider;
        private new Rigidbody rigidbody;
        private float initialDrag;
        private float initialAngularDrag;
        private float minVoxelSize;
        private Vector3 voxelSize;
        private Vector3[] voxels;
        private Vector3 bouyancyForce;

        protected virtual void Awake()
        {
            this.collider = this.GetComponent<Collider>();
            this.rigidbody = this.GetComponent<Rigidbody>();

            this.initialDrag = this.rigidbody.drag;
            this.initialAngularDrag = this.rigidbody.angularDrag;

            var bounds = this.collider.bounds;
            this.voxelSize.x = this.normalizedVoxelSize * bounds.size.x;
            this.voxelSize.y = this.normalizedVoxelSize * bounds.size.y;
            this.voxelSize.z = this.normalizedVoxelSize * bounds.size.z;
            this.minVoxelSize = Mathf.Min(this.voxelSize.x, this.voxelSize.y, this.voxelSize.z);

            if (this.calculateDensity)
            {
                float volume = MathfUtils.CalculateVolume_Mesh(this.GetComponent<MeshFilter>().mesh, this.transform);
                this.density = this.rigidbody.mass / volume;
                Debug.Log(this.density);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (this.water != null && this.voxels.Length > 0)
            {
                Vector3 forceAtSingleVoxel = this.bouyancyForce / this.voxels.Length;

                float submergedVolume = 0f;
                for (int i = 0; i < this.voxels.Length; i++)
                {
                    Vector2 point = this.transform.TransformPoint(this.voxels[i]);
                    float waterLevel = this.water.GetWaterLevel(point);
                    float deepLevel = waterLevel - point.y + (this.minVoxelSize / 2f); // How deep is the point                    
                    float submergedFactor = Mathf.Clamp(deepLevel / this.minVoxelSize, 0f, 1f); // 0 - voxel is fully out of the water, 1 - voxel is fully submerged
                    submergedVolume += submergedFactor;

                    this.rigidbody.AddForceAtPosition(forceAtSingleVoxel * submergedFactor, point);
                }

                // 0 - object is fully out of the water
                // 1 - object is fully submerged
                submergedVolume /= this.voxels.Length;

                this.rigidbody.drag = Mathf.Lerp(this.initialDrag, this.dragInWater, submergedVolume);
                this.rigidbody.angularDrag = Mathf.Lerp(this.initialAngularDrag, this.angularDragInWater, submergedVolume);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(WaterVolume.TAG))
            {
                this.water = other.GetComponent<WaterVolume>();
                if (this.voxels == null)
                {
                    this.voxels = this.CutIntoVoxels();
                }

                this.bouyancyForce = this.CalculateBouyancyForce();
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(WaterVolume.TAG))
            {
                this.water = null;
            }
        }

        private Vector3 CalculateBouyancyForce()
        {
            float objectDensityFactor = 1f / this.density;
            Vector3 bouyancyForce = this.water.Density * this.rigidbody.mass * -Physics.gravity * objectDensityFactor;

            return bouyancyForce;
        }

        private Vector3[] CutIntoVoxels()
        {
            var bounds = this.collider.bounds;
            int xRes = Mathf.RoundToInt(bounds.size.x / this.voxelSize.x);
            int yRes = Mathf.RoundToInt(bounds.size.y / this.voxelSize.y);
            int zRes = Mathf.RoundToInt(bounds.size.z / this.voxelSize.z);
            List<Vector3> voxels = new List<Vector3>(xRes * yRes * zRes);

            for (int ix = 0; ix < xRes; ix++)
            {
                for (int iy = 0; iy < yRes; iy++)
                {
                    for (int iz = 0; iz < zRes; iz++)
                    {
                        float pX = bounds.min.x + this.voxelSize.x * (0.5f + ix);
                        float pY = bounds.min.y + this.voxelSize.y * (0.5f + iy);
                        float pZ = bounds.min.z + this.voxelSize.z * (0.5f + iz);

                        Vector3 point = new Vector3(pX, pY, pZ);
                        if (ColliderUtils.IsPointInsideCollider(point, this.collider, ref bounds))
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
