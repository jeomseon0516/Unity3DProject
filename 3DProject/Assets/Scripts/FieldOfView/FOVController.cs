using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class FOVController : MonoBehaviour
{
    [SerializeField, Range(0.0f, 360.0f)] private float _angle;
    private List<Vector3> _vertices = new List<Vector3>();

    void Start()
    {
        _angle = 72.0f;
    }

    void Update()
    {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = mesh;

        float angle = _angle / 72;

        _vertices.Add(Vector3.zero);

        for (int i = -36; i < 37; ++i)
        {
            _vertices.Add(new Vector3(Mathf.Sin(i * angle * Mathf.Deg2Rad),
                                      0.0f,
                                      Mathf.Cos(i * angle * Mathf.Deg2Rad)) * 5.0f);
        }

        int[] triangles = new int[(_vertices.Count - 2) * 3];

        for (int i = 0; i < _vertices.Count - 2; ++i)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }

        mesh.vertices = _vertices.ToArray();
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        _vertices.Clear();
    }
}
