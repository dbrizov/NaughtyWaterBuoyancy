using UnityEngine;

namespace FluidBuoyancy
{

    public class AffectedGameObject
    {
        public Mesh Mesh { get; set; }
        public Rigidbody Rigidbody { get; set; }

        public AffectedGameObject(Mesh mesh, Rigidbody rigidbody)
        {
            this.Mesh = mesh;
            this.Rigidbody = rigidbody;
        }

        public override int GetHashCode()
        {
            return this.Rigidbody.GetHashCode();
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

            return this.Rigidbody.Equals(objAsAffectedGO.Rigidbody);
        }
    }
}
