using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : IComparable<AStarNode>
{
    public AStarNode Parent { get; set; }
    public Vector3Int NodePoint { get; set; } // 노드의 포인트 Key값으로 쓰인다.
    public Vector3 NodePosition { get; set; }
    public int Cost { get => G + Heuristics; } // F
    public int G { get; set; } // G
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

// 모든 객체가 길을 찾아야할 필요가 있을까?
// pivot객체를 설정해서 주변에 있는 가까운 객체들을 pivot객체를 팔로우하고 pivot이 찾은 노드를 따라 움직일 수 있다.
#if UNITY_EDITOR_WIN
[RequireComponent(typeof(AStarNodeDebug))]
#endif
public class AStar : MonoBehaviour
{
    private const int COST = 10;
    private const int DIAGONAL_COST = 14;
    private const int VERTICAL_DIAGONAL_COST = 18;
    public float SIZE { get => 0.7F; }

    private HashSet<Vector3Int> _closedList = new HashSet<Vector3Int>(); // 키값을 어떻게 가져야만 할까?...
    private Dictionary<Vector3Int, AStarNode> _openList = new Dictionary<Vector3Int, AStarNode>(); // 이미 오픈리스트에 있는 경우 경로 개선을 위해 딕셔너리를 사용
    private PriorityQueue<AStarNode> _openPq  = new PriorityQueue<AStarNode>();
    private Stack<AStarNode> _findList = new Stack<AStarNode>(); // 찾아낸 노드를 순차적으로 가져와 이동할 것이므로 스택을 사용
    [field:SerializeField] public GameObject TargetObject { get; set; }
    public bool IsMove { get; private set; }
    private AStarNode _startNode;
    private AStarNode _endNode;

    private void Start()
    {
        IsMove = false;
    }
    private void FixedUpdate()
    {
        if (TryGetComponent(out AStarNodeDebug debug))
            debug.DrawBox(_findList.ToArray());
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPosition)
    {
        _startNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0, 0);

        Vector3 interval = _startNode.NodePosition - targetPosition;
        Vector3Int nodeInterval = convertIntervalToNodePointInterval(interval);

        Vector3Int endNodePoint = new Vector3Int(interval.x > 0 ? -nodeInterval.x : nodeInterval.x, 0, 
                                                 interval.z > 0 ? -nodeInterval.z : nodeInterval.z);

        _startNode.Heuristics = (nodeInterval.x + nodeInterval.z) * COST;
        _endNode = new AStarNode(null, endNodePoint, _startNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * SIZE, 0, 0);
    }
    private void findWay()
    {
        AStarNode pivotNode = popOpenList();

        while (Vector3.Distance(pivotNode.NodePosition, _endNode.NodePosition) >= SIZE * 0.5f)
        {
            if (!Physics.Raycast(pivotNode.NodePosition,
                (_endNode.NodePosition - pivotNode.NodePosition).normalized,
                Vector3.Distance(pivotNode.NodePosition, _endNode.NodePosition), 
                1 << LayerMask.NameToLayer("Wall")))
            {
                _endNode.Parent = pivotNode;
                pivotNode = _endNode;
                break;
            }

            if (_closedList.Contains(pivotNode.NodePoint))
                continue;

            _closedList.Add(pivotNode.NodePoint);

            int cost = pivotNode.G + COST;
            int diagonalCost = pivotNode.G + DIAGONAL_COST;

            decideMakingNode(pivotNode, new Vector3Int( 0, 0,  1), cost);
            decideMakingNode(pivotNode, new Vector3Int( 1, 0,  0), cost);
            decideMakingNode(pivotNode, new Vector3Int(-1, 0,  0), cost);
            decideMakingNode(pivotNode, new Vector3Int( 0, 0, -1), cost);
            decideMakingNode(pivotNode, new Vector3Int( 1, 0,  1), diagonalCost);
            decideMakingNode(pivotNode, new Vector3Int(-1, 0,  1), diagonalCost);
            decideMakingNode(pivotNode, new Vector3Int( 1, 0, -1), diagonalCost);
            decideMakingNode(pivotNode, new Vector3Int(-1, 0, -1), diagonalCost);

            pivotNode = popOpenList();
        }

        AStarNode temp = pivotNode;
        _findList.Push(pivotNode);

        while (!ReferenceEquals(pivotNode.Parent, null))
        {
            if (Physics.Raycast(pivotNode.Parent.NodePosition,
                (temp.NodePosition - pivotNode.Parent.NodePosition).normalized,
                Vector3.Distance(temp.NodePosition, pivotNode.Parent.NodePosition),
                1 << LayerMask.NameToLayer("Wall")))
            {
                temp = pivotNode;
                _findList.Push(pivotNode);
            }

            pivotNode = pivotNode.Parent;
        }
    }
    private void decideMakingNode(AStarNode pivotNode, Vector3Int nodePoint, int cost)
    {
        Vector3 position = pivotNode.NodePosition + (Vector3)nodePoint * SIZE;
        Vector3Int newNodePoint = pivotNode.NodePoint + nodePoint;

        if (_closedList.Contains(newNodePoint) || Physics.CheckSphere(position, SIZE, 1 << LayerMask.NameToLayer("Wall")))
            return;

        if (getAStarNode(pivotNode, out AStarNode newNode, position, newNodePoint, cost))
            pushOpenList(newNode);
        else // 새로 만들고자 하는 노드의 위치가 이미 오픈리스트에 존재한다면? 경로 개선을 한다.
            wayReformation(pivotNode, newNode, cost);
    }
    // 경로 개선 함수
    private void wayReformation(AStarNode pivotNode, AStarNode node, int g)
    {
        if (g >= node.G) return;

        node.Parent = pivotNode;
        node.G = g;
    }
    private void pushOpenList(AStarNode node)
    {
        _openPq.Push(node);
        _openList.Add(node.NodePoint, node);
    }
    private AStarNode popOpenList()
    {
        AStarNode node = _openPq.Pop();
        _openList.Remove(node.NodePoint);

        return node;
    }
    // pivotNode 기준으로 nodePoint를 받아와 해당 위치에 새로운 노드 생성 또는 기존 노드가 존재한다면 경로 개선
    private bool getAStarNode(AStarNode pivotNode, out AStarNode newNode, Vector3 position, Vector3Int nodePoint, int g)
    {
        if (_openList.TryGetValue(nodePoint, out AStarNode oldNode))
        {
            newNode = oldNode;
            return false;
        }

        newNode = new AStarNode(pivotNode, nodePoint, position, g, getManhattanDistance(_endNode.NodePosition - position) * COST);
        return true;
    }
    private int getManhattanDistance(Vector3 interval)
    {
        Vector3Int nodeInterval = convertIntervalToNodePointInterval(interval);
        return nodeInterval.x + nodeInterval.z;
    }
    private Vector3Int convertIntervalToNodePointInterval(Vector3 interval)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(interval.x) / SIZE);
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z) / SIZE);

        return new Vector3Int(x, 0, z);
    }
    public void FindPath()
    {
        makeStartNodeEndNode(TargetObject.transform.position);
        _findList.Clear();

        pushOpenList(_startNode);
        findWay();

        _openPq.Clear();
        _openList.Clear();
        _closedList.Clear();

        if (TryGetComponent(out AStarNodeDebug debug))
            debug.UpdateGizmo(_findList.ToArray(), _startNode, _endNode, SIZE);
    }
    public bool ContainWays() 
    { 
        return _findList.Count > 0; 
    }
    public Vector3 GetMoveNext()
    {
        return _findList.Pop().NodePosition;
    }
}