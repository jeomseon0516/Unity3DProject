using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AStarNodeDebug : MonoBehaviour
{
    private const int VERTEX_COUNT = 8;
    private Vector3[] _vertices = new Vector3[VERTEX_COUNT];
    public AStar _aStar { get; set; }

    private void Awake()
    {
        TryGetComponent(out AStar aStar);
        _aStar = aStar;
    }
    private void Start()
    {
        foreach (AStarNode node in _aStar.Nodes)
        {
            Vector3 startPoint = new Vector3(
                node.NodePoint.x - _aStar.RADIUS * 0.5f,
                node.NodePoint.z - _aStar.RADIUS * 0.5f,
                node.NodePoint.y - _aStar.RADIUS * 0.5f);

            _vertices[0] = startPoint;
            _vertices[1] = startPoint + new Vector3(_aStar.RADIUS, 0.0f, 0.0f);
            _vertices[2] = startPoint + new Vector3(_aStar.RADIUS, 0.0f, _aStar.RADIUS);
            _vertices[3] = startPoint + new Vector3(0.0f, 0.0f, _aStar.RADIUS);
            _vertices[4] = startPoint + new Vector3(0.0f, _aStar.RADIUS, 0.0f);
            _vertices[5] = startPoint + new Vector3(_aStar.RADIUS, _aStar.RADIUS, 0.0f);
            _vertices[6] = startPoint + new Vector3(_aStar.RADIUS, _aStar.RADIUS, _aStar.RADIUS);
            _vertices[7] = startPoint + new Vector3(0.0f, _aStar.RADIUS, _aStar.RADIUS);
        }
    }

    private void Update()
    {
        DrawBox();
    }

    private void DrawBox()
    {
        for (int i = 0; i < 4; ++i)
        {
            Debug.DrawLine(_vertices[i], _vertices[i + 4]);
            Debug.DrawLine(_vertices[i + 4], _vertices[(i + 1) % 4 + 4]);
            Debug.DrawLine(_vertices[i], _vertices[(i + 1) % 4]);
        }
    }
}