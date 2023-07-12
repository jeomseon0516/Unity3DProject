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

    private HashSet<Vector3Int> _closedList = new HashSet<Vector3Int>(); // 키값을 어떻게 가져야만 할까?...
    private Dictionary<Vector3Int, AStarNode> _openList = new Dictionary<Vector3Int, AStarNode>(); // 이미 오픈리스트에 있는 경우를 체크하기 위해서
    private PriorityQueue<AStarNode> _openPq  = new PriorityQueue<AStarNode>();
    public Stack<AStarNode> FindList { get; } = new Stack<AStarNode>(); // 찾아낸 노드를 순차적으로 가져와 이동할 것이므로 스택을 사용
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

        if (IsFind)
            IsFind = false;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!IsFind)
            {
                IsFind = true;

                makeStartNodeEndNode(TargetObject.transform.position);
                FindList.Clear();

                PushOpenList(StartNode);
                findWay();

                _openPq.Clear();
                _openList.Clear();
                _closedList.Clear();
            }
        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPosition)
    {
        StartNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0, 0);

        Vector3 interval = StartNode.NodePosition - targetPosition;
        Vector3Int nodeInterval = convertNodePointIntervalFromInterval(interval);

        Vector3Int endNodePoint = new Vector3Int(interval.x > 0 ? -nodeInterval.x : nodeInterval.x, 0, 
                                                 interval.z > 0 ? -nodeInterval.z : nodeInterval.z);

        StartNode.Heuristics = (nodeInterval.x + nodeInterval.z) * COST;
        EndNode = new AStarNode(null, endNodePoint, StartNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * SIZE, 0, 0);
    }
    private void findWay()
    {
        AStarNode pivotNode = null;

        for (pivotNode = PopOpenList(); Vector3.Distance(pivotNode.NodePosition, EndNode.NodePosition) >= SIZE * 0.5f; pivotNode = PopOpenList())
        {
            if (!Physics.Raycast(pivotNode.NodePosition,
                (EndNode.NodePosition - pivotNode.NodePosition).normalized,
                Vector3.Distance(pivotNode.NodePosition, EndNode.NodePosition)
                , 1 << LayerMask.NameToLayer("Wall")))
            {
                EndNode.Parent = pivotNode;
                pivotNode = EndNode;
                break;
            }

            if (!_closedList.Contains(pivotNode.NodePoint))
            {
                _closedList.Add(pivotNode.NodePoint);

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
        }

        AStarNode temp = pivotNode;
        while (!ReferenceEquals(pivotNode.Parent, null))
        {
            if (Physics.Raycast(pivotNode.Parent.NodePosition,
                (temp.NodePosition - pivotNode.Parent.NodePosition).normalized,
                Vector3.Distance(temp.NodePosition, pivotNode.Parent.NodePosition),
                1 << LayerMask.NameToLayer("Wall")))
            {
                temp = pivotNode;
                FindList.Push(pivotNode);
            }

            pivotNode = pivotNode.Parent;
        }
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
    // pivotNode 기준으로 nodePoint를 받아와 해당 위치에 새로운 노드 생성 또는 기존 노드가 존재한다면 경로 개선
    private bool checkMakeAndAStarNode(AStarNode pivotNode, out AStarNode newNode, Vector3Int nodePoint, int g)
    {
        Vector3 position = pivotNode.NodePosition + (Vector3)nodePoint * SIZE;
        Vector3Int newNodePoint = pivotNode.NodePoint + nodePoint;

        newNode = null;

        // closedList에 이미 요소가 들어있거나, 생성할 노드의 위치에 다른 어떤 물체가 있다면?
        if (_closedList.Contains(newNodePoint) || Physics.CheckSphere(position, SIZE, 1 << LayerMask.NameToLayer("Wall")))
            return false;

        // 새로 만들고자 하는 노드의 위치가 이미 오픈리스트에 존재한다면? 경로 개선을 한다.
        if (_openList.TryGetValue(newNodePoint, out AStarNode oldNode))
        {
            if (g < oldNode.G)
            {
                oldNode.Parent = pivotNode;
                oldNode.G = g;
            }

            return false;
        }

        newNode = new AStarNode(pivotNode, newNodePoint, position, g, getManhattanDistance(EndNode.NodePosition - position) * COST);
        return true;
    }
    private int getManhattanDistance(Vector3 interval)
    {
        Vector3Int nodeInterval = convertNodePointIntervalFromInterval(interval);
        return nodeInterval.x + nodeInterval.z;
    }
    private Vector3Int convertNodePointIntervalFromInterval(Vector3 interval)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(interval.x) / SIZE);
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z) / SIZE);

        return new Vector3Int(x, 0, z);
    }
}