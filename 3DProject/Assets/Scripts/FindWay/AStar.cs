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
    private const int VERTICAL_DIAGONAL_COST = 18;
    public float SIZE { get => 0.4F; }
    [field:SerializeField] public List<AStarNode> Nodes { get; private set; } = new List<AStarNode>();
    [field:SerializeField] public GameObject TargetObject { get; set; }
    public bool IsFind { get; private set; }
    public AStarNode StartNode { get; private set; } 
    public AStarNode EndNode { get; private set; }

    void Start()
    {
        IsFind = false;
    }
    void Update()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        if (!IsFind)
        {
            IsFind = true;
            makeStartNodeEndNode();
        }
        else
        {

        }
    }
    // 생성은 스타트 노드 기준으로 잡는다.
    void makeStartNodeEndNode()
    {
        StartNode = new AStarNode(null, transform.position);

        Vector3 interval = StartNode.NodePoint - TargetObject.transform.position;

        float x = Mathf.RoundToInt(Mathf.Abs(interval.x / SIZE));
        float z = Mathf.RoundToInt(Mathf.Abs(interval.z / SIZE));

        Vector3 endPoint = StartNode.NodePoint + new Vector3(interval.x < 0 ? x : -x, 0.0f, interval.z < 0 ? z : -z) * SIZE;
        EndNode = new AStarNode(null, endPoint);
    }
}
