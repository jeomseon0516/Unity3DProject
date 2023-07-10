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
    public int Cost { get => G + Heuristics; } // G
    public int G { get; set; }
    public int Heuristics { get; set; } // H
    public AStarNode(AStarNode parent, Vector3Int nodePoint, Vector3 nodePosition, int g, int heuristics)
    {
        Parent = parent;
        NodePoint = nodePoint;
        NodePosition = nodePosition;
        Heuristics = heuristics;
        G = g;
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
    private Dictionary<Vector3Int, AStarNode> _openList   = new Dictionary<Vector3Int, AStarNode>(); // 이미 오픈리스트에 있는 경우를 체크하기 위해서
    private PriorityQueue<AStarNode> _openPq  = new PriorityQueue<AStarNode>();
    public List<AStarNode> FindList { get; } = new List<AStarNode>();
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

            PushOpenList(StartNode);
            findWay();

            _openPq.Clear();
            _openList.Clear();
            _closedList.Clear();
        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPosition)
    {
        StartNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0, 0);

        Vector3 interval = StartNode.NodePosition - targetPosition;
        int[] h = getG(interval);

        int x = h[0];
        int z = h[1];

        Vector3Int endNodePoint = new Vector3Int(interval.x > 0 ? -x : x, 0, interval.z > 0 ? -z : z);
        StartNode.Heuristics = (Mathf.Abs(endNodePoint.x) + Mathf.Abs(endNodePoint.z)) * COST;

        EndNode = new AStarNode(null, endNodePoint, StartNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * SIZE, 0, 0);
    }
    private void findWay()
    {
        AStarNode pivotNode = PopOpenList();

        if (Vector3.Distance(pivotNode.NodePosition, EndNode.NodePosition) < SIZE * 0.5f)
        {
            while (!ReferenceEquals(pivotNode.Parent, null))
            {
                FindList.Add(pivotNode);
                pivotNode = pivotNode.Parent;
            }

            return;
        }

        if (!_closedList.ContainsKey(pivotNode.NodePoint))
        {
            _closedList.Add(pivotNode.NodePoint, pivotNode);

            if (checkMakeAndAStarNode(pivotNode, out AStarNode centerTopNode, new Vector3Int(0, 0, 1), pivotNode.G + COST))
                PushOpenList(centerTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftCenterNode, new Vector3Int(1, 0, 0), pivotNode.G + COST))
                PushOpenList(leftCenterNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightCenterNode, new Vector3Int(-1, 0, 0), pivotNode.G + COST))
                PushOpenList(rightCenterNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode centerBottomNode, new Vector3Int(0, 0, -1), pivotNode.G + COST))
                PushOpenList(centerBottomNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftTopNode, new Vector3Int(1, 0, 1), pivotNode.G + DIAGONAL_COST))
                PushOpenList(leftTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightTopNode, new Vector3Int(-1, 0, 1), pivotNode.G + DIAGONAL_COST))
                PushOpenList(rightTopNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode leftBottomNode, new Vector3Int(1, 0, -1), pivotNode.G + DIAGONAL_COST))
                PushOpenList(leftBottomNode);
            if (checkMakeAndAStarNode(pivotNode, out AStarNode rightBottomNode, new Vector3Int(-1, 0, -1), pivotNode.G + DIAGONAL_COST))
                PushOpenList(rightBottomNode);
        }

        findWay();
    }
    private void PushOpenList(AStarNode node)
    {
        _openPq.Push(node);
        _openList.Add(node.NodePoint, node);
    }
    private AStarNode PopOpenList()
    {
        AStarNode node = _openPq.Pop();
        _openList.Remove(node.NodePoint);

        return node;
    }
    private bool checkMakeAndAStarNode(AStarNode pivotNode, out AStarNode newNode, Vector3Int nodePoint, int g)
    {
        Vector3 position = pivotNode.NodePosition + (Vector3)nodePoint * SIZE;
        Vector3Int newNodePoint = pivotNode.NodePoint + nodePoint;

        float distance = Vector3.Distance(pivotNode.NodePosition, position);

        // closedList에 이미 요소가 들어있거나, 생성할 노드의 위치에 다른 어떤 물체가 있다면?
        
        if (_closedList.ContainsKey(newNodePoint) ||
             Physics.CheckSphere(
                 pivotNode.NodePosition,
                 SIZE,
                 1 << LayerMask.NameToLayer("Wall"))
             )
        {
            newNode = null;
            return false;
        }

        // 경로 개선
        if (_openList.TryGetValue(newNodePoint, out AStarNode oldNode))
        {
            int[] goal = getG(pivotNode.NodePosition - position);

            int goalSum = goal[0] + goal[1];

            if (newG < oldNode.G)
            {
                oldNode.Parent = pivotNode;
                oldNode.G = newG;
            }

            newNode = null;
            return false;
        }
        else
        {
            int[] h = getG(EndNode.NodePosition - position);
            newNode = new AStarNode(pivotNode, newNodePoint, position, g, (h[0] + h[1]) * COST);

            return true;
        }
    }
    private int[] getG(Vector3 interval)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(interval.x) / SIZE);
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z) / SIZE);

        return new int[2] { x, z };
    }
}