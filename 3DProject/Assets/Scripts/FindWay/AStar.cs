using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 노드는 그리드맵으로 구현되어 있다.
public class AStarNode : IComparable<AStarNode>
{
    public AStarNode Parent { get; set; }
    public Vector3 NodePoint { get; set; } 
    public int Cost { get; set; } // G
    public int Heuristics { get; set; } // H
    public AStarNode(AStarNode parent, Vector3 nodePoint, int cost)
    {
        Parent = parent;
        NodePoint = nodePoint;
        Cost = cost;
    }
    // 1이 true -1 이 false // 현재 
    public int CompareTo(AStarNode other)
    {
        return other.Cost > Cost ? 1 : -1;
    }
}

#if UNITY_EDITOR_WIN
[RequireComponent(typeof(AStarNodeDebug))]
#endif
public class AStar : MonoBehaviour
{
    private const int COST = 10;
    private const int DIAGONAL_COST = 14;
    private const int VERTICAL_DIAGONAL_COST = 18;
    public float SIZE { get => 0.4F; }
    [field:SerializeField] public List<AStarNode> Nodes { get; private set; } = new List<AStarNode>();
    [field:SerializeField] public GameObject TargetObject { get; set; }

    private PriorityQueue<AStarNode> _openList  = new PriorityQueue<AStarNode>();
    private List<AStarNode> _closedList = new List<AStarNode>();
    private List<AStarNode> _findList = new List<AStarNode>();

    public bool IsFind { get; private set; }
    public bool IsMove { get; private set; }
    public AStarNode StartNode { get; private set; } 
    public AStarNode EndNode { get; private set; }

    private void Start()
    {
        IsFind = false;
    }
    private void Update()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        if (!IsFind)
        {
            IsFind = true;
            makeStartNodeEndNode(TargetObject.transform.position);
            _openList.Push(StartNode);
            _findList.Add(findWay());
        }
        else
        {
            
        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPoint)
    {
        StartNode = new AStarNode(null, transform.position, 0);

        int[] g = GetG(StartNode, targetPoint, out Vector3 interval);

        int x = g[0];
        int z = g[1];

        Vector3 endPoint = StartNode.NodePoint + new Vector3(interval.x < 0 ? x : -x, 0.0f, interval.z < 0 ? z : -z) * SIZE;
        EndNode = new AStarNode(null, endPoint, (x + z) * COST);
    }
    private AStarNode findWay()
    {
        AStarNode pivotNode = _openList.Pop();
        _closedList.Add(pivotNode);

        /*
         * 0 1 2
         * 3 p 4
         * 5 6 7
        */
        PriorityQueue<AStarNode> nodes = new PriorityQueue<AStarNode>();

        if (!(Vector3.Distance(pivotNode.NodePoint, EndNode.NodePoint) < SIZE * 0.5f))
        {
            GetAStarNode(pivotNode, new Vector3(SIZE,  0.0f, SIZE),  pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(0.0f,  0.0f, SIZE),  pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(-SIZE, 0.0f, SIZE),  pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(SIZE,  0.0f, 0.0f),  pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(-SIZE, 0.0f, 0.0f),  pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(SIZE,  0.0f, -SIZE), pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(0.0f,  0.0f, -SIZE), pivotNode.Heuristics);
            GetAStarNode(pivotNode, new Vector3(-SIZE, 0.0f, -SIZE), pivotNode.Heuristics);

            if ()

            _findList.Add(findWay());
        }
        else
            _findList.Add(EndNode);

        return pivotNode.Parent;
    }
    private int[] GetG(AStarNode pivotNode, Vector3 targetPoint, out Vector3 interval)
    {
        interval = pivotNode.NodePoint - targetPoint;

        int x = Mathf.RoundToInt(Mathf.Abs(interval.x / SIZE));
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z / SIZE));

        return new int[2] { x, z };
    }

    private AStarNode GetAStarNode(AStarNode pivotNode, Vector3 position, int heuristics)
    {
        Vector3 nodePoint = pivotNode.NodePoint + position;
        int[] g = GetG(pivotNode, nodePoint, out Vector3 interval);

        return new AStarNode(pivotNode, nodePoint, g[0] + g[1] + heuristics);
    }
}
