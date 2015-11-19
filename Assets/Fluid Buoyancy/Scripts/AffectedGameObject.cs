using UnityEngine;

namespace FluidBuoyancy
{
    public class AffectedGameObject
    {
        private GameObject gameObject;
        private Mesh mesh;
        private Rigidbody rigidbody;
        private BoxCollider boxCollider;

        public AffectedGameObject(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }

        public GameObject GameObject
        {
            get
            {
                return this.gameObject;
            }
            private set
            {
                this.gameObject = value;
            }
        }

        public Mesh Mesh
        {
            get
            {
                if (this.mesh == null)
                {
                    this.mesh = this.GameObject.GetComponent<MeshFilter>().mesh;
                }

                return this.mesh;
            }
        }

        public Rigidbody Rigidbody
        {
            get
            {
                if (this.rigidbody == null)
                {
                    this.rigidbody = this.GameObject.GetComponent<Rigidbody>();
                }

                return this.rigidbody;
            }
        }

        public BoxCollider BoxCollider
        {
            get
            {
                if (this.boxCollider == null)
                {
                    this.boxCollider = this.GameObject.GetComponent<BoxCollider>();
                }

                return this.boxCollider;
            }
        }

        public override int GetHashCode()
        {
            return this.GameObject.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var objAsAffectedGO = obj as AffectedGameObject;
            if (objAsAffectedGO == null)
            {
                return false;
            }

            return this.GameObject.Equals(objAsAffectedGO.GameObject);
        }
    }
}
