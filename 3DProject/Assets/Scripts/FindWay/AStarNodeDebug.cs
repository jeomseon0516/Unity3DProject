using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNodeDebug : MonoBehaviour
{
    private const int START = 0;
    private const int END = 1;
    private const int VERTEX_COUNT = 8;

    private List<Vector3[]> _vertices = new List<Vector3[]>();
    private List<Vector3[]> _pivotNodes = new List<Vector3[]>() { new Vector3[VERTEX_COUNT], new Vector3[VERTEX_COUNT] };
    private bool _isFind;
    public AStar _aStar { get; private set; }

    private void Awake()
    {
        TryGetComponent(out AStar aStar);
        _aStar = aStar;
    }
    private void Start()
    {
        _isFind = false;
    }
    private void Update()
    {
        if (_aStar.IsFind && !_isFind)
        {
            print("?");

            foreach (AStarNode node in _aStar.Nodes)
                _vertices.Add(createVertices(convertNodePoint(node)));

            _pivotNodes[START] = createVertices(convertNodePoint(_aStar.StartNode));
            _pivotNodes[END]   = createVertices(convertNodePoint(_aStar.EndNode));
        }

        foreach (Vector3[] vertices in _vertices)
            drawBox(vertices, Color.black);

        drawBox(_pivotNodes[START], Color.blue);
        drawBox(_pivotNodes[END], Color.red);

        _isFind = _aStar.IsFind;
    }

    private void drawBox(Vector3[] vertices, Color color)
    {
        for (int i = 0; i < 4; ++i)
        {
            Debug.DrawLine(vertices[i], vertices[i + 4], color);
            Debug.DrawLine(vertices[i], vertices[(i + 1) % 4], color);
            Debug.DrawLine(vertices[i + 4], vertices[(i + 1) % 4 + 4], color);
        }
    }
    private Vector3 convertNodePoint(AStarNode node)
    {
        return new Vector3(
            node.NodePoint.x - _aStar.SIZE * 0.5f,
            node.NodePoint.y - _aStar.SIZE * 0.5f,
            node.NodePoint.z - _aStar.SIZE * 0.5f);
    }
    private Vector3[] createVertices(Vector3 pivotPoint)
    {
        Vector3[] vertices = new Vector3[VERTEX_COUNT];

        vertices[0] = pivotPoint;
        vertices[1] = pivotPoint + new Vector3(_aStar.SIZE, 0.0f, 0.0f);
        vertices[2] = pivotPoint + new Vector3(_aStar.SIZE, 0.0f, _aStar.SIZE);
        vertices[3] = pivotPoint + new Vector3(0.0f, 0.0f, _aStar.SIZE);
        vertices[4] = pivotPoint + new Vector3(0.0f, _aStar.SIZE, 0.0f);
        vertices[5] = pivotPoint + new Vector3(_aStar.SIZE, _aStar.SIZE, 0.0f);
        vertices[6] = pivotPoint + new Vector3(_aStar.SIZE, _aStar.SIZE, _aStar.SIZE);
        vertices[7] = pivotPoint + new Vector3(0.0f, _aStar.SIZE, _aStar.SIZE);

        return vertices;
    }
}