using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AStarNodeDebug : MonoBehaviour
{
    private const int VERTEX_COUNT = 8;
    private Vector3[] _vertices = new Vector3[VERTEX_COUNT];
    public AStar _aStar { get; set; }
    void Start()
    {
        TryGetComponent(out AStar aStar);
        _aStar = aStar;
    }

    void Update()
    {
        
    }
}
