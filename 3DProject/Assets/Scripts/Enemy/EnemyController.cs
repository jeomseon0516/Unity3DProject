using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public partial class EnemyController : MonoBehaviour
{
    [field: SerializeField, Range(10, 50)] private float Speed { get; set; } = 10.0f;
    [field: SerializeField, Range(0, 360)] private float Angle { get; set; } = 90;

    private bool isMove;

    private void Awake()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GameObject parentNode = GameObject.Find("NAKZI");

        if (parentNode)
        {
            parentNode.transform.GetChild(0).TryGetComponent(out Node target);
            TargetNode = target;
        }
    }
    void Start()
    {
        RayCount = 5;

        isMove = true;
        Angle = 45;
        Scale = 1.25f;

        StartCoroutine(SetRotation());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            FindWay();

        Move();
    }
    private void FixedUpdate()
    {
        float pivotRadian = CustomMath.ConvertFromAngleToRadian(transform.eulerAngles.y);
        float intervalAngle = Angle / RayCount;
        float radian = -(CustomMath.ConvertFromAngleToRadian(RayCount * 0.5f * intervalAngle));

        for (int i = 0; i < RayCount + 1; ++i)
        {
            float calcRadian = pivotRadian + radian;
            radian += CustomMath.ConvertFromAngleToRadian(intervalAngle);

            Vector3 direction = new Vector3(Mathf.Sin(calcRadian), 0, Mathf.Cos(calcRadian));

            Debug.DrawRay(transform.position, direction * 5.0f, Color.red);
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 5.0f))
            {

            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (ReferenceEquals(TargetNode.gameObject, other.gameObject))
        {
            StartCoroutine(SetRotation());
            if (TargetNode.Next)
                TargetNode = TargetNode.Next;
        }
    }
    void Move()
    {
        if (!TargetNode) return;

        Vector3 direction = (TargetNode.transform.position - transform.position).normalized;

        if (isMove)
            transform.position += direction * Speed * Time.deltaTime;
    }

    IEnumerator SetRotation()
    {
        float time;
        isMove = true;

        int count = UnityEngine.Random.Range(2, 4) + 1;

        // 고개 돌리기
        //for (int i = 0; i < count; ++i)
        //{
        //    time = 0.0f;

        //    float radian = CustomMath.ConvertFromAngleToRadian(transform.eulerAngles.y + Random.Range(-Angle, Angle));

        //    while (time < 1.0f)
        //    {
        //        transform.rotation = Quaternion.Lerp(
        //            transform.rotation,
        //            Quaternion.LookRotation(new Vector3(Mathf.Sin(radian), 0.0f, Mathf.Cos(radian))),
        //            0.016f);

        //        time += Time.deltaTime;
        //        yield return null;
        //    }
        //}

        time = 0.0f;

        while (time < 1.0f)
        {
            if (TargetNode)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation((TargetNode.transform.position - transform.position).normalized),
                    0.016f);

                time += Time.deltaTime;
            }

            yield return null;
        }

        isMove = true;
    }
}

// .. 길찾기 기능
public partial class EnemyController : MonoBehaviour
{
    const int T = 1;
    const int R = 2;
    const int S = 3;
    const int M = 0;

    delegate void pathFindingMethod(List<Node> openList, Node node, ref Node pivotNode, ref Node endNode, 
        ref float pivotDistance, ref float endDistance, float targetDistance, float distance);
    [field: SerializeField, Range(1, 10)] private float Scale { get; set; } = 1.25f;
    [field: SerializeField, Range(2, 360)] private int RayCount { get; set; }
    [field: SerializeField] private List<Vector3> Vertices { get; set; }
    [field: SerializeField] public GameObject Target { get; private set; }
    public Node TargetNode { get; private set; }
    public Node CurrentNode { get; private set; }

    // .. pivotNode, EndNode, openList ..

    void InitPathFinding(List<Node> openList, Node node, ref Node pivotNode, ref Node endNode,
        ref float pivotDistance, ref float endDistance, float targetDistance, float distance)
    {
        if (endDistance > targetDistance)
        {
            endNode = node;
            endDistance = targetDistance;
        }

        if (pivotDistance > distance)
        {
            if (pivotNode)
            {
                openList.Add(pivotNode);
                pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.blue;
            }

            pivotNode = node;
            pivotDistance = distance;
        }
        else
        {
            openList.Add(node);
            node.GetComponent<MyGizmo>().GizmoColor = Color.blue;
        }
    }
    Tuple<Node, Node, List<Node>> SettingPathFinding(GameObject obj, pathFindingMethod action)
    {
        // .. 어떤 노드가 나랑 가장 가까운지 찾아주는
        float pivotDistance = Mathf.Infinity, endDistance = Mathf.Infinity;

        Node pivotNode = null, endNode = null; // .. 피봇 노드는 현재 기준이 되는 노드 피봇 노드를 기준으로 코스트를 계산하여 최적의 노드를 찾고 해당 노드는 다음 pivotNode가 된다.

        List<Node> openList = new List<Node>(); // .. 다음 pivot 노드가 될 수 있는 후보 목록

        foreach (Vector3 vertex in GetVertices(obj))
        {
            if (vertex.y >= transform.position.y + 0.05f ||
                vertex.y + 0.05f <= transform.position.y) continue;

            Vector3 wayPoint = ConvertFromVertexToVector(obj, vertex, Scale);

            if (!Vertices.Contains(wayPoint))
            {
                Node node = new GameObject("Node_" + Vertices.Count.ToString()).AddComponent<Node>();

                node.gameObject.AddComponent<MyGizmo>();
                node.transform.position = wayPoint;

                float distance = CustomMath.GetDistance(node.transform.position.z, transform.position.z,
                                                        node.transform.position.x, transform.position.x);

                float targetDistance = CustomMath.GetDistance(node.transform.position.z, Target.transform.position.z,
                                                              node.transform.position.x, Target.transform.position.x);

                // .. 딜리게이트 사용해야 할듯 중간에 장애물을 만나면 다시 버텍스를 구해와야하는데 endNode와 startNode는 변하지 않기 때문
                action(openList, node, ref pivotNode, ref endNode, ref pivotDistance, ref endDistance, targetDistance, distance);
                Vertices.Add(wayPoint);
            }
        }

        return new Tuple<Node, Node, List<Node>>(pivotNode, endNode, openList);
    }

    void FindWay()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity) ||
            hit.transform.name.Contains("Node_") ||
            !Target) return;

        Vertices.Clear();

        // .. 어떤 노드가 가장 가까운지 찾아주는
        Tuple<Node, Node, List<Node>> tuple = SettingPathFinding(hit.transform.gameObject, InitPathFinding);

        Node pivotNode = tuple.Item1, endNode = tuple.Item2;
        Node startNode = pivotNode;

        List<Node> openList = tuple.Item3;
        List<GameObject> colList = new List<GameObject>();

        colList.Add(hit.transform.gameObject);

        if (pivotNode)
            pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.red;

        if (endNode)
        {
            endNode.Next = new GameObject("EndNode").AddComponent<Node>();
            endNode.Next.transform.position = Target.transform.position;
        }

        TargetNode = PathFindingNodes(openList, new List<Node>(openList), startNode, pivotNode, endNode, colList);
    }

    // 노드의 길을 찾은 후 StartNode 반환
    Node PathFindingNodes(List<Node> oldList, List<Node> openList, Node startNode, Node pivotNode, Node endNode, List<GameObject> colList)
    {
        float distance = Mathf.Infinity;
        float startTargetDistance = distance;
        int index = -1;

        for (int i = 0; i < openList.Count; ++i)
        {
            float intervalDistance = CustomMath.GetDistance(pivotNode.transform.position.z, openList[i].transform.position.z,
                                                            pivotNode.transform.position.x, openList[i].transform.position.x);

            // .. 현재 pivot이 되는 노드가 검사할 openList의 노드사이에 무언가 있는지 검사 뭔가 있다면? 해당 노드는 제외
            bool isCollision = Physics.Raycast(
                    pivotNode.transform.position,
                    (openList[i].transform.position - pivotNode.transform.position).normalized,
                    out RaycastHit checkHit,
                    intervalDistance
                );

            if (isCollision)
            {
                if (!checkHit.transform.name.Contains("Node_") && !colList.Contains(checkHit.transform.gameObject))
                {
                    Tuple<Node, Node, List<Node>> tuple = SettingPathFinding(checkHit.transform.gameObject,
                        (List<Node> checkList, Node node, ref Node pivotNode, ref Node endNode,
                         ref float pivotDistance, ref float endDistance, float targetDistance, float distance) =>
                        {
                            checkList.Add(node);
                            node.GetComponent<MyGizmo>().GizmoColor = Color.blue;
                        });

                    colList.Add(checkHit.transform.gameObject);

                    oldList.AddRange(tuple.Item3);
                    openList.Clear();
                    openList.AddRange(oldList);

                    print(openList.Count);
                    index = -1;
                    pivotNode = startNode;

                    break;
                }
                else
                    continue;
            }

            float targetDistance = CustomMath.GetDistance(Target.transform.position.z, openList[i].transform.position.z,
                                                          Target.transform.position.x, openList[i].transform.position.x);

            if (distance > intervalDistance)
            {
                distance = intervalDistance;
                index = i;
            }
            else if (distance == intervalDistance && startTargetDistance > targetDistance)
            {
                startTargetDistance = targetDistance;
                index = i;
            }
        }

        if (index >= 0)
        {
            pivotNode.Next = openList[index];
            openList[index].Parent = pivotNode;
            pivotNode = openList[index];
            pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.green;
            openList.Remove(openList[index]);
        }

        if (!pivotNode.Equals(endNode))
            PathFindingNodes(oldList, openList, startNode, pivotNode, endNode, colList);

        while (pivotNode.Parent)
            pivotNode = pivotNode.Parent;

        return pivotNode;
    }

    Vector3 ConvertFromVertexToVector(GameObject obj, Vector3 vertexPoint, float scale)
    {
        Matrix4x4[] matrix = new Matrix4x4[4];

        matrix[T] = Matrix.Translate(new Vector3(obj.transform.position.x, 0.0f, obj.transform.position.z));
        matrix[R] = Matrix.Rotate(obj.transform.eulerAngles);
        matrix[S] = Matrix.Scale(new Vector3(obj.transform.lossyScale.x, 0.0f, obj.transform.lossyScale.z) * scale);
        matrix[M] = matrix[T] * matrix[R] * matrix[S];

        return matrix[M].MultiplyPoint(vertexPoint);
    }

    List<Vector3> GetVertices(GameObject hitObject)
    {
        List<Vector3> vertexList = new List<Vector3>();

        if (hitObject.transform.childCount != 0)
            for (int i = 0; i < hitObject.transform.childCount; ++i)
                vertexList.AddRange(GetVertices(hitObject.transform.GetChild(i).gameObject));

        if (!hitObject.transform.gameObject.TryGetComponent(out MeshFilter meshFilter))
            return vertexList;

        vertexList.AddRange(meshFilter.mesh.vertices);

        return vertexList;
    }
    void OutMatrix(Matrix4x4 m)
    {
        Debug.Log("===================================================");
        Debug.Log(m.m00 + ", " + m.m01 + ", " + m.m02 + ", " + m.m03);
        Debug.Log(m.m10 + ", " + m.m11 + ", " + m.m12 + ", " + m.m13);
        Debug.Log(m.m20 + ", " + m.m21 + ", " + m.m22 + ", " + m.m23);
        Debug.Log(m.m30 + ", " + m.m31 + ", " + m.m32 + ", " + m.m33);
        Debug.Log("===================================================");
    }
}