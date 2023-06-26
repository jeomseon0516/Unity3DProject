using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    public AStarNode Parent { get; set; }
    public Vector3 NodePoint { get; set; } 
    public float Cost { get; set; }

    public AStarNode(AStarNode parent, Vector3 nodePoint)
    {
        Parent = parent;
        NodePoint = nodePoint;
        Cost = 0.0f;
    }
}

#if UNITY_EDITOR_WIN
[RequireComponent(typeof(AStarNodeDebug))]
#endif
public class AStar : MonoBehaviour
{
    private const int COST = 10;
    private const int DIAGONAL_COST = 14;
    public float RADIUS { get => 0.5F; }

    AStarNode startNode, endNode;
    [field:SerializeField] public List<AStarNode> Nodes { get; private set; } = new List<AStarNode>();
    [field:SerializeField] public GameObject TargetObject { get; set; }

    private void Awake()
    {
        makeStartNodeEndNode();
    }
    void Start()
    {
    }
    void Update()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;


    }
    void makeStartNodeEndNode()
    {
        startNode = new AStarNode(null, transform.position);
        Nodes.Add(startNode);
    }
}
