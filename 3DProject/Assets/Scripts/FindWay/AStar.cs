using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : IComparable<AStarNode>
{
    public AStarNode Parent { get; set; }
    public Vector3Int NodePoint { get; set; } // .. 노드의 포인트 Key값으로 쓰인다.
    public Vector3 NodePosition { get; set; }
    public int Cost { get => G + Heuristics; } // .. F
    public int G { get; set; } // .. G
    public int Heuristics { get; set; } // .. H
    public AStarNode(AStarNode parent, Vector3Int nodePoint, Vector3 nodePosition, int g, int heuristics)
    {
        Parent = parent;
        NodePoint = nodePoint;
        NodePosition = nodePosition;
        Heuristics = heuristics;
        G = g;
    }
    // .. 1이 true -1 이 false // 현재 
    public int CompareTo(AStarNode other)
    {
        return other.Cost > Cost ? 1 : -1;
    }
}

// .. 모든 객체가 길을 찾아야할 필요가 있을까?
// .. pivot객체를 설정해서 주변에 있는 가까운 객체들을 pivot객체를 팔로우하고 pivot이 찾은 노드를 따라 움직일 수 있다.
#if UNITY_EDITOR_WIN
[RequireComponent(typeof(AStarNodeDebug))]
#endif
public class AStar : MonoBehaviour
{
    private const int COST = 10; // .. 직선 코스트
    private const int DIAGONAL_COST = 14; // .. 평면상의 대각선 코스트
    private const int VERTICAL_DIAGONAL_COST = 18; // .. 3차원 상의 대각선 코스트
    public float Size { get; set; } // .. 복셀의 해상도라고 생각하여야 한다. 사이즈가 작아질 수록 연산량이 기하급수적으로 커지기 때문에 잘 사용하여야 한다.

    /* 
     * .. 부동 소수점 문제로 노드 자체의 Vector3를 Key로 가지고 있다면 문제가 생길 수 있기때문에 Vector3Int로 관리
     * Vector3Int는 실 좌표가 아닌 StartNode를 기준으로 어느정도의 위치에 있는지를 판단하는 임의의 값
     */
    // .. 닫힌 목록은 노드의 정보가 필요하지 않기때문에 key값만 가진 해쉬셋으로 구성
    private HashSet<Vector3Int> _closedList = new HashSet<Vector3Int>(); 

    /* 
     * .. 열린 목록은 경로 개선할때 노드의 f값 변경이 일어나기 때문에 Key와 Value로 구성된 딕셔너리 사용 
     * 우선순위 큐와 딕셔너리에 있는 노드의 갯수는 항상 같아야 하며 두 자료구조를 한개처럼 생각하여 관리하여야 한다.
     */

    private Dictionary<Vector3Int, AStarNode> _openList = new Dictionary<Vector3Int, AStarNode>(); 
    // .. 우선순위 큐로 항상 가장 최적의 값을 찾아온다.  
    private PriorityQueue<AStarNode> _openPq = new PriorityQueue<AStarNode>();
    // .. 찾아낸 노드를 순차적으로 가져와 이동할 것이므로 스택을 사용
    private Stack<AStarNode> _findList = new Stack<AStarNode>(); 
    [field:SerializeField] public GameObject TargetObject { get; set; } // .. 타겟이 될 오브젝트 또는 타겟이 될 (Vector3) 좌표를 가지고 있는다.
    public bool IsMove { get; private set; }

    // .. 탐색을 시작할 노드
    private AStarNode _startNode;
    // .. 탐색을 끝낼때 사용할 노드
    private AStarNode _endNode;

    private void Start()
    {
        IsMove = false;
        Size = 0.7f;
    }
#if UNITY_EDITOR_WIN
    private void FixedUpdate()
    {
        if (TryGetComponent(out AStarNodeDebug debug))
            debug.DrawBox(_findList.ToArray());
    }
#endif
    private void findWay(Vector3 targetPosition)
    {
        _startNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0, 0);

        Vector3 interval = _startNode.NodePosition - targetPosition;
        Vector3Int nodeInterval = convertIntervalToNodePointInterval(interval);

        Vector3Int endNodePoint = new Vector3Int(interval.x > 0 ? -nodeInterval.x : nodeInterval.x, 0,
                                                 interval.z > 0 ? -nodeInterval.z : nodeInterval.z);

        _startNode.Heuristics = (nodeInterval.x + nodeInterval.z) * COST;
        _endNode = new AStarNode(null, endNodePoint, _startNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * Size, 0, 0);

        pushOpenList(_startNode);

        AStarNode pivotNode = popOpenList();

        while (Vector3.Distance(pivotNode.NodePosition, _endNode.NodePosition) >= Size * 0.5f)
        {
            if (!Physics.Raycast(pivotNode.NodePosition,
                (_endNode.NodePosition - pivotNode.NodePosition).normalized,
                Vector3.Distance(pivotNode.NodePosition, _endNode.NodePosition),
                1 << LayerMask.NameToLayer("Wall"))) // 엔드노드까지 레이캐스트 장애물이 존재하지 않을 경우 해당 노드까지가 베스트 경로 이므로 길찾기 종료 
            {
                _endNode.Parent = pivotNode;
                pivotNode = _endNode;
                break;
            }

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
            /* 
             * .. 불필요한 경로 제거 복셀 데이터로 길찾기를 하기 때문에 A*만으로 찾은 경로는 최단거리가 아님 
             * 직선 거리가 항상 최단 거리 라는 점을 이용해 레이캐스트 현재 노드의 부모 노드가 레이캐스트를 하는 기준이 되는 노드 사이에 만약 장애물이 없으면 스택에 넣지 않음 
             * 만약 장애물이 존재한다면 기준이 되는 노드 갱신 후 피벗 노드는 Push해줌
             * 해당 방식을 이용하면 최단 거리가 나오게 된다.
             */
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
        Vector3 position = pivotNode.NodePosition + (Vector3)nodePoint * Size; // 새로 생성한 노드의 포지션이 될 값
        Vector3Int newNodePoint = pivotNode.NodePoint + nodePoint;

        if (_closedList.Contains(newNodePoint) || Physics.CheckSphere(position, Size, 1 << LayerMask.NameToLayer("Wall")))
            return;

        if (_openList.TryGetValue(newNodePoint, out AStarNode node))
        {
            if (cost < node.G) // .. 경로 개선
            {
                node.Parent = pivotNode;
                node.G = cost;
            }
        }
        else
            pushOpenList(new AStarNode(pivotNode, newNodePoint, position, cost, getManhattanDistance(_endNode.NodePosition - position) * COST));
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
    private int getManhattanDistance(Vector3 interval)
    {
        Vector3Int nodeInterval = convertIntervalToNodePointInterval(interval);
        return nodeInterval.x + nodeInterval.z;
    }
    private Vector3Int convertIntervalToNodePointInterval(Vector3 interval)
    {
        int x = Mathf.RoundToInt(Mathf.Abs(interval.x) / Size);
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z) / Size);

        return new Vector3Int(x, 0, z);
    }
    public void FindPath()
    {
        _findList.Clear();

        findWay(TargetObject.transform.position);

        _openPq.Clear();
        _openList.Clear();
        _closedList.Clear();

#if UNITY_EDITOR_WIN
        if (TryGetComponent(out AStarNodeDebug debug))
            debug.UpdateGizmo(_findList.ToArray(), _startNode, _endNode, Size);
#endif
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