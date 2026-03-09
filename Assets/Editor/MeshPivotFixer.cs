using UnityEngine;
using UnityEditor;

public class MeshPivotFixer : EditorWindow {
    [MenuItem("Tools/Fix Quad Pivot")]
    public static void FixQuad() {
        GameObject obj = Selection.activeGameObject;
        if (obj == null || obj.GetComponent<MeshFilter>() == null) return;

        MeshFilter mf = obj.GetComponent<MeshFilter>();
        Mesh originalMesh = mf.sharedMesh;
        Mesh newMesh = Instantiate(originalMesh);

        Vector3[] verts = newMesh.vertices;
        // On décale les points du mesh vers le haut de 0.5 unité
        for (int i = 0; i < verts.Length; i++) {
            verts[i].y += 0.4f; 
        }

        newMesh.vertices = verts;
        newMesh.RecalculateBounds();

        // On sauvegarde ce nouveau mesh dans tes fichiers
        AssetDatabase.CreateAsset(newMesh, "Assets/Quad_Pivot_Bottom.asset");
        AssetDatabase.SaveAssets();

        mf.sharedMesh = newMesh;
        Debug.Log("Nouveau mesh créé : Assets/Quad_Pivot_Bottom.asset");
    }
}