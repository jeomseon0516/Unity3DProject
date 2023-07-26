using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VerticesTo
{
    public static float GetHeightFromVertices(GameObject obj)
    {
        List<Vector3> vertices = GetVerticesFromSkinndedMeshRenderer(obj);
        vertices.AddRange(GetVerticesFromMeshFilter(obj));

        if (vertices.Count == 0)
        {
#if UNITY_EDITOR_WIN
            Debug.Log("vertices not found!");
#endif
            return 0.0f;
        }

        float maxPivot = float.NegativeInfinity;
        float minPivot = float.PositiveInfinity;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 point = obj.transform.TransformPoint(vertex);

            if (point.y > maxPivot)
                maxPivot = point.y;
            if (point.y < minPivot)
                minPivot = point.y;
        }
        
        return maxPivot - minPivot;
    }
    public static List<Vector3> GetVerticesFromSkinndedMeshRenderer(GameObject obj)
    {
        List<Vector3> vertices = new List<Vector3>();

        SkinnedMeshRenderer[] filters = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in filters)
            vertices.AddRange(skinnedMeshRenderer.sharedMesh.vertices);

        return vertices;
    }
    public static List<Vector3> GetVerticesFromMeshFilter(GameObject obj)
    {
        List<Vector3> vertices = new List<Vector3>();

        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in filters)
            vertices.AddRange(meshFilter.mesh.vertices);

        return vertices;
    }
}
