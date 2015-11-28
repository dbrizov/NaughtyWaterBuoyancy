using UnityEditor;
using UnityEngine;

namespace WaterBuoyancy
{
    [CustomEditor(typeof(WaterVolume))]
    public class WaterVolumeEditor : Editor
    {
        private const float BOX_COLLIDER_HEIGHT = 5f;

        private WaterVolume waterVolumeTarget;
        private SerializedProperty density;
        private SerializedProperty rows;
        private SerializedProperty columns;
        private SerializedProperty quadSegmentSize;
        //private SerializedProperty debugTrans;

        [MenuItem("Water Bouyancy/Create Water Mesh")]
        private static void CreateMesh()
        {
            Mesh mesh = WaterMeshGenerator.GenerateMesh(5, 5, 1f);
            AssetDatabase.CreateAsset(mesh, "Assets/Water Buoyancy/Models/Water Mesh.asset");
        }

        protected virtual void OnEnable()
        {
            this.waterVolumeTarget = (WaterVolume)this.target;

            this.density = this.serializedObject.FindProperty("density");
            this.rows = this.serializedObject.FindProperty("rows");
            this.columns = this.serializedObject.FindProperty("columns");
            this.quadSegmentSize = this.serializedObject.FindProperty("quadSegmentSize");
            //this.debugTrans = this.serializedObject.FindProperty("debugTrans");

            Undo.undoRedoPerformed += this.OnUndoRedoPerformed;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= this.OnUndoRedoPerformed;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.density);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.rows);
            EditorGUILayout.PropertyField(this.columns);
            EditorGUILayout.PropertyField(this.quadSegmentSize);
            if (EditorGUI.EndChangeCheck())
            {
                this.rows.intValue = Mathf.Max(1, this.rows.intValue);
                this.columns.intValue = Mathf.Max(1, this.columns.intValue);
                this.quadSegmentSize.floatValue = Mathf.Max(0f, this.quadSegmentSize.floatValue);

                this.UpdateMesh(this.rows.intValue, this.columns.intValue, this.quadSegmentSize.floatValue);
                this.UpdateBoxCollider(this.rows.intValue, this.columns.intValue, this.quadSegmentSize.floatValue);
            }

            //EditorGUILayout.PropertyField(this.debugTrans);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void UpdateMesh(int rows, int columns, float quadSegmentSize)
        {
            if (Application.isPlaying)
            {
                return;
            }

            MeshFilter meshFilter = this.waterVolumeTarget.GetComponent<MeshFilter>();
            Mesh oldMesh = meshFilter.sharedMesh;

            Mesh newMesh = WaterMeshGenerator.GenerateMesh(rows, columns, quadSegmentSize);
            newMesh.name = "Water Mesh Instance";

            meshFilter.sharedMesh = newMesh;

            EditorUtility.SetDirty(meshFilter);

            if (oldMesh != null && !AssetDatabase.Contains(oldMesh))
            {
                DestroyImmediate(oldMesh);
            }
        }

        private void UpdateBoxCollider(int rows, int columns, float quadSegmentSize)
        {
            var boxCollider = this.waterVolumeTarget.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Vector3 size = new Vector3(columns * quadSegmentSize, BOX_COLLIDER_HEIGHT, rows * quadSegmentSize);
                boxCollider.size = size;

                Vector3 center = size / 2f;
                center.y *= -1f;
                boxCollider.center = center;

                EditorUtility.SetDirty(boxCollider);
            }
        }

        private void OnUndoRedoPerformed()
        {
            this.UpdateMesh(this.waterVolumeTarget.Rows, this.waterVolumeTarget.Columns, this.waterVolumeTarget.QuadSegmentSize);
            this.UpdateBoxCollider(this.waterVolumeTarget.Rows, this.waterVolumeTarget.Columns, this.waterVolumeTarget.QuadSegmentSize);
        }
    }
}
