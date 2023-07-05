using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class FOVController : MonoBehaviour
{
    [SerializeField] private float _angle;
    private List<Vector3> _vertices = new List<Vector3>();

    void Start()
    {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(Vector3.zero);

        _angle = 360.0f;
        float angle = _angle / 72;

        for (int i = 0; i < 72; ++i)
        {
            vertices.Add(new Vector3(Mathf.Sin(i * 5.0f * Mathf.Deg2Rad),
                                     0.0f,
                                     Mathf.Cos(i * 5.0f * Mathf.Deg2Rad)) * 5.0f);
        }

        vertices.Add(vertices[1]);

        int[] triangles = new int[(vertices.Count - 2) * 3];

        for (int i = 0; i < vertices.Count - 2; ++i)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    void Update()
    {
        //Mesh mesh = new Mesh();
        //MeshFilter meshFilter = GetComponent<MeshFilter>();

        //meshFilter.mesh = mesh;

        //List<Vector3> vertices = new List<Vector3>();

        //vertices.Add(Vector3.zero);

        //for (int i = 0; i < 72; ++i)
        //{
        //    vertices.Add(new Vector3(Mathf.Sin(i * 5.0f * Mathf.Deg2Rad),
        //                             0.0f,
        //                             Mathf.Cos(i * 5.0f * Mathf.Deg2Rad)) * 5.0f);
        //}

        //int[] triangles = new int[(vertices.Count - 1) * 3];

        //for (int i = 0; i < vertices.Count - 2; ++i)
        //{
        //    int index = i * 3;
        //    triangles[index] = 0;
        //    triangles[index + 1] = i + 1;
        //    triangles[index + 2] = i + 2;
        //}

        //int lastIndex = (vertices.Count - 1) * 3 - 3;

        //triangles[lastIndex] = 0;
        //triangles[lastIndex + 1] = 72;
        //triangles[lastIndex + 2] = 1;

        //mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles;

        //mesh.RecalculateNormals();
    }
}
