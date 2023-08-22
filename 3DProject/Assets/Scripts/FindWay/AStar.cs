using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : IComparable<AStarNode>
{
    public AStarNode Parent { get; internal set; }
    public Vector3Int NodePoint { get; internal set; } // .. 노드의 포인트 Key값으로 쓰인다.
    public Vector3 NodePosition { get; internal set; }
    public int Cost { get; internal set; } // .. F
    public int G { get; internal set; } // .. G
    public int Heuristics { get; internal set; } // .. H
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

/*
 * .. 모든 객체가 길을 찾아야할 필요가 있을까?
 * pivot객체를 설정해서 주변에 있는 가까운 객체들을 pivot객체를 팔로우하고 pivot이 찾은 노드를 따라 움직일 수 있다.
 * 해당 방식은 Y축을 고려하지 않은 방식이다. 복셀 데이터 형태로 탐색 시 Y축까지 고려하면 매 검사때마다 주변 노드 26곳을 탐색해야하는데 이것은 지금의 탐색비용보다 3배 큰 비용이다.
 * 현재 방식은 내 주변에 어떤 노드가 있는지 알 수 없으므로 매 길찾기마다 새로운 노드를 생성하는 방식으로 길찾기를 진행한다. 메모리를 계속해서 할당하였다가 삭제하기를 반복하고
 * 오브젝트 풀링 방식으로 사용하기에도 큰 메모리 공간을 차지할 수 있는 리스크가 있어서 사용하지 않았기 때문에 몬스터 수가 조금만 증가해도 렉을 유발하는 원인이 될 것 이다.
 * 최적화 방식을 여러가지 고민해 볼 수 있다.
 * 
 * (1) - 맵의 메쉬를 가져와 주변 메쉬를 노드로 만들어 A* 탐색을 하는 방식이다. 
 *      
 *      1. 유니티에서 자체 지원하는 네비게이션 메쉬가 그 방식을 따르고 있다. 
 *         하지만 맵 오브젝트가 여러 개의 네비 메쉬로 나뉘어져 있는 경우 계속해서 새로 베이크 해주어야하고 
 *         동적으로 움직이는 오브젝트에 대응하기 힘든 문제도 있다. 사용하지 않음
 *      2. 네비 매쉬를 자체 구현하여 동적인 오브젝트에 대응하는 방식도 있다.
 *         주변 노드를 확실히 알고 있고 다리와 같이 Y축의 차이가 있고 X, Z축이 겹치는 경우에도 손쉽게 대응 할 수 있다.
 *         조건을 걸어서 동적인 오브젝트가 일정 부피 이상을 차지하고 있는 방식을 판별하여 해당 노드는 이동할 수 없는 노드로 만들 수도 있다.
 *
 * 맵을 스캔해서 미리 노드들을 로드하거나 만드는 방식으로 바꾸어 연산량을 줄여야한다. 
 * 
 */
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
     * 
     * 닫힌 목록은 노드의 정보가 필요하지 않기때문에 key값만 가진 해쉬셋으로 구성
     */
    private HashSet<Vector3Int> m_closedList = new HashSet<Vector3Int>(); 
    /* 
     * .. 열린 목록은 경로 개선할때 노드의 f값 변경이 일어나기 때문에 Key와 Value로 구성된 딕셔너리 사용 
     * 우선순위 큐와 딕셔너리에 있는 노드의 갯수는 항상 같아야 하며 두 자료구조를 한개처럼 생각하여 관리하여야 한다.
     */
    private Dictionary<Vector3Int, AStarNode> m_openList = new Dictionary<Vector3Int, AStarNode>(); 
    // .. 우선순위 큐로 항상 추정 비용이 가장 적은 노드의 값을 찾아온다.  
    private PriorityQueue<AStarNode> m_openPq = new PriorityQueue<AStarNode>();
    // .. 찾아낸 노드를 순차적으로 가져와 이동할 것이므로 스택을 사용
    private Stack<AStarNode> m_findList = new Stack<AStarNode>(); 
    // .. 탐색을 시작할 노드
    private AStarNode m_startNode;
    // .. 탐색을 끝낼때 사용할 노드
    private AStarNode m_endNode;
    // .. 타겟이 될 오브젝트 또는 타겟이 될 (Vector3) 좌표를 가지고 있는다.
    [field:SerializeField] public GameObject TargetObject { get; set; } 
    public bool IsMove { get; private set; }

    private void Start()
    {
        IsMove = false;
        Size = 0.7f;
    }
#if UNITY_EDITOR_WIN
    private void FixedUpdate()
    {
        if (TryGetComponent(out AStarNodeDebug debug))
            debug.DrawBox(m_findList.ToArray());
    }
#endif
    private void findWay(Vector3 targetPosition)
    {
        m_startNode = new AStarNode(null, new Vector3Int(0, 0, 0), transform.position, 0, 0);

        Vector3 interval = m_startNode.NodePosition - targetPosition;
        Vector3Int nodeInterval = convertIntervalToNodePointInterval(interval);

        Vector3Int endNodePoint = new Vector3Int(interval.x > 0 ? -nodeInterval.x : nodeInterval.x, 0,
                                                 interval.z > 0 ? -nodeInterval.z : nodeInterval.z);

        m_startNode.Heuristics = (nodeInterval.x + nodeInterval.z) * COST;
        m_endNode = new AStarNode(null, endNodePoint, m_startNode.NodePosition + new Vector3(endNodePoint.x, 0.0f, endNodePoint.z) * Size, 0, 0);

        pushOpenList(m_startNode);

        AStarNode pivotNode = popOpenList();

        while (Vector3.Distance(pivotNode.NodePosition, m_endNode.NodePosition) >= Size * 0.5f)
        {
            if (!Physics.Raycast(pivotNode.NodePosition,
                (m_endNode.NodePosition - pivotNode.NodePosition).normalized,
                Vector3.Distance(pivotNode.NodePosition, m_endNode.NodePosition),
                1 << LayerMask.NameToLayer("Wall"))) // .. 엔드노드까지 레이캐스트 장애물이 존재하지 않을 경우 해당 노드까지가 베스트 경로 이므로 길찾기 종료 
            {
                m_endNode.Parent = pivotNode;
                pivotNode = m_endNode;
                break;
            }

            m_closedList.Add(pivotNode.NodePoint);

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
        m_findList.Push(pivotNode);

        while (!ReferenceEquals(pivotNode.Parent, null))
        {
            /* 
             * .. 불필요한 경로 제거 복셀 데이터로 길찾기를 하기 때문에 A*만으로 찾은 경로는 최단거리가 아님 
             * 직선 거리가 항상 최단 거리 라는 점을 이용해 레이를 쏴 현재 노드의 부모 노드가 레이캐스트를 하는 기준이 되는 노드 사이에 만약 장애물이 없으면 스택에 넣지 않음 
             * 만약 장애물이 존재한다면 기준이 되는 노드 갱신 후 피벗 노드는 Push해줌
             * 해당 방식을 이용하면 최단 거리가 나오게 된다.
             */
            if (Physics.Raycast(pivotNode.Parent.NodePosition,
                (temp.NodePosition - pivotNode.Parent.NodePosition).normalized,
                Vector3.Distance(temp.NodePosition, pivotNode.Parent.NodePosition),
                1 << LayerMask.NameToLayer("Wall"))) 
            {
                temp = pivotNode;
                m_findList.Push(pivotNode);
            }

            pivotNode = pivotNode.Parent;
        }
    }

    // .. 노드를 만들어야 할 때 노드는 클래스이므로 가비지 생성을 방지하기 위해 포지션 값으로 노드가 있는지 없는지 검사후 최종적으로 노드를 생성한다.
    private void decideMakingNode(AStarNode pivotNode, Vector3Int nodePoint, int cost)
    {
        Vector3 position = pivotNode.NodePosition + (Vector3)nodePoint * Size; // .. 새로 생성한 노드의 포지션이 될 값
        Vector3Int newNodePoint = pivotNode.NodePoint + nodePoint;

        if (m_closedList.Contains(newNodePoint) || Physics.CheckSphere(position, Size, 1 << LayerMask.NameToLayer("Wall")))
            return;

        if (m_openList.TryGetValue(newNodePoint, out AStarNode node))
        {
            if (cost < node.G) // .. 경로 개선
            {
                node.Parent = pivotNode;
                node.G = cost;
            }
        }
        else
            pushOpenList(new AStarNode(pivotNode, newNodePoint, position, cost, getManhattanDistance(m_endNode.NodePosition - position) * COST));
    }
    private void pushOpenList(AStarNode node)
    {
        m_openPq.Push(node);
        m_openList.Add(node.NodePoint, node);
    }
    private AStarNode popOpenList()
    {
        AStarNode node = m_openPq.Pop();
        m_openList.Remove(node.NodePoint);

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
        m_findList.Clear(); // .. 기존에 찾았던 노드들을 초기화.

        // .. 길찾기 수행
        findWay(TargetObject.transform.position);

        // .. 길찾기 수행후 더 이상 필요없는 노드나 요소들 제거
        m_openPq.Clear();
        m_openList.Clear();
        m_closedList.Clear();

#if UNITY_EDITOR_WIN
        if (TryGetComponent(out AStarNodeDebug debug))
            debug.UpdateGizmo(m_findList.ToArray(), m_startNode, m_endNode, Size);
#endif
    }
    public bool ContainWays()
    {
        return m_findList.Count > 0;
    }
    public Vector3 GetMoveNext()
    {
        return m_findList.Pop().NodePosition;
    }
}