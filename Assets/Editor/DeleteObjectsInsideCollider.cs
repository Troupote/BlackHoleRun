using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MeshCollider))]
public class DeleteInsideNonConvexMesh : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🗑 Delete Objects INSIDE Mesh (non-convex)", EditorStyles.boldLabel);

        if (GUILayout.Button("Delete Objects Inside Collider"))
        {
            DeleteInside();
        }
    }

    void DeleteInside()
    {
        MeshCollider meshCollider = (MeshCollider)target;
        Transform meshTransform = meshCollider.transform;

        if (meshCollider.convex)
        {
            Debug.LogWarning("This method is for NON-CONVEX MeshColliders. Disable 'Convex' first.");
            return;
        }

        int deleted = 0;

        for(int i=0; i < meshTransform.childCount; i++)
        {
            GameObject go = meshTransform.GetChild(i).gameObject;
            if (go == meshCollider.gameObject) continue;

            if (IsPointInsideMeshCollider(go.transform.position, meshCollider))
            {
                Undo.DestroyObjectImmediate(go);
                deleted++;
            }
        }

        Debug.Log($"✅ Deleted {deleted} GameObjects whose pivot is inside the non-convex MeshCollider.");
    }

    public static bool IsPointInsideMeshCollider(Vector3 point, MeshCollider meshCollider)
    {
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            Debug.LogWarning("MeshCollider or sharedMesh is null.");
            return false;
        }

        Vector3[] directions = { Vector3.right, Vector3.up, Vector3.forward };
        int insideCount = 0;

        foreach (Vector3 dir in directions)
        {
            Ray ray = new Ray(point - dir * 1000f, dir);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);

            int count = 0;
            foreach (var hit in hits)
            {
                if (hit.collider == meshCollider)
                    count++;
            }

            if (count % 2 == 1)
                insideCount++;
        }

        // Considérer que le point est à l'intérieur s'il est "dedans" dans au moins 2 directions
        return insideCount >= 2;
    }
}
