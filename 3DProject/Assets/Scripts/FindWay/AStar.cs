using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 노드는 그리드맵으로 구현되어 있다.
public class AStarNode : IComparable<AStarNode>
{
    public AStarNode Parent { get; set; }
    public Vector3Int NodePoint { get; set; } // 노드의 포인트 Key값으로 쓰인다.
    public Vector3 NodePosition { get; set; }
    public int Cost { get; set; } // G
    public int Heuristics { get; set; } // H
    public AStarNode(AStarNode parent, Vector3Int nodePoint, Vector3 nodePosition, int cost)
    {
        Parent = parent;
        NodePoint = nodePoint;
        NodePosition = nodePosition;
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

    private Dictionary<Vector3Int, AStarNode> _closedList = new Dictionary<Vector3Int, AStarNode>(); // 키값을 어떻게 가져야만 할까?...
    private PriorityQueue<AStarNode> _openList  = new PriorityQueue<AStarNode>();
    public List<AStarNode> FindList { get; private set; } = new List<AStarNode>();

    [field:SerializeField] public List<AStarNode> Nodes { get; private set; } = new List<AStarNode>();
    [field:SerializeField] public GameObject TargetObject { get; set; }
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
             findWay();
        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPosition)
    {
        StartNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0);

        Vector3Int endNodePoint = getNodePoint(StartNode, targetPosition, out int[] g);
        EndNode = new AStarNode(null, endNodePoint, StartNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * SIZE, (g[0] + g[1]) * COST);
    }
    private void findWay()
    {
        AStarNode pivotNode = _openList.Pop();

        if (!_closedList.ContainsKey(pivotNode.NodePoint))
            _closedList.Add(pivotNode.NodePoint, pivotNode);

        if (!(Vector3.Distance(pivotNode.NodePoint, EndNode.NodePoint) < SIZE * 0.5f))
        {
            if (checkMakeAndAStarNode(pivotNode, out AStarNode centerTopNode, pivotNode.NodePosition + new Vector3(0.0f, 0.0f, SIZE), pivotNode.Heuristics))
                _openList.Push(centerTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftCenterNode, pivotNode.NodePosition + new Vector3(SIZE, 0.0f, 0.0f), pivotNode.Heuristics))
                _openList.Push(leftCenterNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightCenterNode, pivotNode.NodePosition + new Vector3(-SIZE, 0.0f, 0.0f), pivotNode.Heuristics))
                _openList.Push(rightCenterNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode centerBottomNode, pivotNode.NodePosition + new Vector3(0.0f, 0.0f, -SIZE), pivotNode.Heuristics))
                _openList.Push(centerBottomNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftTopNode, pivotNode.NodePosition + new Vector3(SIZE, 0.0f, SIZE), pivotNode.Heuristics))
                _openList.Push(leftTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightTopNode, pivotNode.NodePosition + new Vector3(-SIZE, 0.0f, SIZE), pivotNode.Heuristics))
                _openList.Push(rightTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftBottomNode, pivotNode.NodePosition + new Vector3(SIZE, 0.0f, -SIZE), pivotNode.Heuristics))
                _openList.Push(leftBottomNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightBottomNode, pivotNode.NodePosition + new Vector3(-SIZE, 0.0f, -SIZE), pivotNode.Heuristics))
                _openList.Push(rightBottomNode);

            findWay();
        }

        FindList.Add(pivotNode);
    }
    private bool checkMakeAndAStarNode(AStarNode pivotNode, out AStarNode newNode, Vector3 position, int heuristics)
    {
        Vector3Int nodePoint = getNodePoint(pivotNode, position, out int[] g);

        float distance = Vector3.Distance(pivotNode.NodePosition, position);

        // closedList에 이미 요소가 들어있거나, 생성할 노드의 위치에 다른 어떤 물체가 있다면?
        if (_closedList.ContainsKey(nodePoint) || 
                Physics.Raycast(
                    pivotNode.NodePosition, 
                    (position - pivotNode.NodePosition).normalized, 
                    distance, 
                    1 << LayerMask.NameToLayer("Wall"))
                )
        {
            newNode = null;
            return false;
        }

        newNode = new AStarNode(pivotNode, nodePoint, position, (g[0] + g[1]) * COST + heuristics);

        return true;
    }
    private int[] getG(AStarNode pivotNode, Vector3 targetPosition, out Vector3 interval)
    {
        interval = pivotNode.NodePosition - targetPosition;

        int x = Mathf.RoundToInt(Mathf.Abs(interval.x / SIZE));
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z / SIZE));

        return new int[2] { x, z };
    }
    private Vector3Int getNodePoint(AStarNode pivotNode, Vector3 position, out int[] g)
    {
        g = getG(pivotNode, position, out Vector3 interval);

        int x = g[0];
        int z = g[1];

        return new Vector3Int(interval.x < 0 ? x : -x, 0, interval.z < 0 ? z : -z);
    }
}