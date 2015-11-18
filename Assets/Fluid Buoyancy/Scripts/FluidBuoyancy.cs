using UnityEngine;
using System.Collections.Generic;

namespace FluidBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    public class FluidBuoyancy : MonoBehaviour
    {
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
            foreach (var affectedGOById in this.affectedGameObjectById)
            {
                var rigidbody = affectedGOById.Value.Rigidbody;
                rigidbody.AddForce(new Vector3(0f, this.force, 0f));
                rigidbody.AddForce(rigidbody.velocity * -1f * this.viscosity);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (this.IsValidGameObject(other.gameObject))
            {
                var affectedGO = new AffectedGameObject(other.GetComponent<MeshFilter>().mesh, other.GetComponent<Rigidbody>());
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
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
