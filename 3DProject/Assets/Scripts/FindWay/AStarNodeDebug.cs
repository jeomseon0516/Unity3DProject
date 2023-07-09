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
    private List<Vector3[]> _findNodes = new List<Vector3[]>();

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
            foreach (AStarNode node in _aStar.Nodes)
                _vertices.Add(createVertices(convertNodePoint(node)));

            foreach (AStarNode node in _aStar.FindList)
                _findNodes.Add(createVertices(convertNodePoint(node)));

            _pivotNodes[START] = createVertices(convertNodePoint(_aStar.StartNode));
            _pivotNodes[END]   = createVertices(convertNodePoint(_aStar.EndNode));
        }

        foreach (Vector3[] vertices in _findNodes)
            drawBox(vertices, Color.green);

        drawBox(_pivotNodes[START], Color.blue);
        drawBox(_pivotNodes[END],   Color.red);

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
            node.NodePosition.x - _aStar.SIZE * 0.5f,
            node.NodePosition.y - _aStar.SIZE * 0.5f,
            node.NodePosition.z - _aStar.SIZE * 0.5f);
    }
    private Vector3[] createVertices(Vector3 pivotPosition)
    {
        Vector3[] vertices = new Vector3[VERTEX_COUNT];

        vertices[0] = pivotPosition;
        vertices[1] = pivotPosition + new Vector3(_aStar.SIZE, 0.0f, 0.0f);
        vertices[2] = pivotPosition + new Vector3(_aStar.SIZE, 0.0f, _aStar.SIZE);
        vertices[3] = pivotPosition + new Vector3(0.0f, 0.0f, _aStar.SIZE);
        vertices[4] = pivotPosition + new Vector3(0.0f, _aStar.SIZE, 0.0f);
        vertices[5] = pivotPosition + new Vector3(_aStar.SIZE, _aStar.SIZE, 0.0f);
        vertices[6] = pivotPosition + new Vector3(_aStar.SIZE, _aStar.SIZE, _aStar.SIZE);
        vertices[7] = pivotPosition + new Vector3(0.0f, _aStar.SIZE, _aStar.SIZE);

        return vertices;
    }
}