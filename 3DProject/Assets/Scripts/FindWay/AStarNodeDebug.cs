using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNodeDebug : MonoBehaviour
{
    private const int VERTEX_COUNT = 8;
    private const int START = 0;
    private const int END = 1;

    private List<Vector3[]> _pivotNodes = new List<Vector3[]>() { new Vector3[VERTEX_COUNT], new Vector3[VERTEX_COUNT] };
    private List<Vector3[]> _findNodes = new List<Vector3[]>();

    private bool _isFind;

    public void DrawBox(AStarNode[] findNodes)
    {
        for (int i = 0; i < findNodes.Length - 1; ++i)
            Debug.DrawRay(findNodes[i].NodePosition,
                (findNodes[i + 1].NodePosition - findNodes[i].NodePosition).normalized * Vector3.Distance(findNodes[i + 1].NodePosition, findNodes[i].NodePosition),
                Color.red);

        foreach (Vector3[] vertices in _findNodes)
            drawBox(vertices, Color.green);

        drawBox(_pivotNodes[START], Color.blue);
        drawBox(_pivotNodes[END], Color.red);
    }
    // .. Stack -> ToArray..
    public void UpdateGizmo(AStarNode[] findNodes, AStarNode startNode, AStarNode endNode, float size)
    {
        _findNodes.Clear();

        foreach (AStarNode node in findNodes)
            _findNodes.Add(createVertices(convertNodePoint(node, size), size));

        _pivotNodes[START] = createVertices(convertNodePoint(startNode, size), size);
        _pivotNodes[END] = createVertices(convertNodePoint(endNode, size), size);
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
    private Vector3 convertNodePoint(AStarNode node, float size)
    {
        return new Vector3(
            node.NodePosition.x - size * 0.5f,
            node.NodePosition.y - size * 0.5f,
            node.NodePosition.z - size * 0.5f);
    }
    private Vector3[] createVertices(Vector3 pivotPosition, float size)
    {
        Vector3[] vertices = new Vector3[VERTEX_COUNT];

        vertices[0] = pivotPosition;
        vertices[1] = pivotPosition + new Vector3(size, 0.0f, 0.0f);
        vertices[2] = pivotPosition + new Vector3(size, 0.0f, size);
        vertices[3] = pivotPosition + new Vector3(0.0f, 0.0f, size);
        vertices[4] = pivotPosition + new Vector3(0.0f, size, 0.0f);
        vertices[5] = pivotPosition + new Vector3(size, size, 0.0f);
        vertices[6] = pivotPosition + new Vector3(size, size, size);
        vertices[7] = pivotPosition + new Vector3(0.0f, size, size);

        return vertices;
    }
}