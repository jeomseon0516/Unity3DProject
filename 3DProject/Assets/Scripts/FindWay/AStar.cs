using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    public AStarNode Parent { get; set; }
    public Vector3 NodePoint { get; set; } 
    public int Cost { get; set; } // G

    public AStarNode(AStarNode parent, Vector3 nodePoint, int cost)
    {
        Parent = parent;
        NodePoint = nodePoint;
        Cost = cost;
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

    private List<AStarNode> _openList   = new List<AStarNode>();
    private List<AStarNode> _closedList = new List<AStarNode>();

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
            findWay(null, StartNode);
        }
        else
        {
            
        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    private void makeStartNodeEndNode(Vector3 targetPoint)
    {
        StartNode = new AStarNode(null, transform.position, 0);

        Vector3 interval = StartNode.NodePoint - targetPoint;

        int x = Mathf.RoundToInt(Mathf.Abs(interval.x / SIZE));
        int z = Mathf.RoundToInt(Mathf.Abs(interval.z / SIZE));

        Vector3 endPoint = StartNode.NodePoint + new Vector3(interval.x < 0 ? x : -x, 0.0f, interval.z < 0 ? z : -z) * SIZE;
        EndNode = new AStarNode(null, endPoint, (x + z) * COST);

        print(EndNode.Cost);
    }
    private void findWay(AStarNode parentNode, AStarNode pivotNode)
    {
        _closedList.Add(pivotNode);

        // 여기서 검사
        for (int i = 0; i < 9; ++i)
        {
            pivotNode.
        }
    }
}
