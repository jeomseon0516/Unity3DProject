using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VerticesTo
{
    public static float GetHeightFromVertices(GameObject obj)
    {
        List<Vector3> vertices = GetVertices(obj);

        if (vertices.Count == 0)
        {
            Debug.Log(vertices.Count);
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

    public static List<Vector3> GetVertices(GameObject obj)
    {
        List<Vector3> vertices = new List<Vector3>();

        SkinnedMeshRenderer[] filters = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer mf in filters)
            vertices.AddRange(mf.sharedMesh.vertices);

        return vertices;
    }
}
