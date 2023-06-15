using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    const int T = 1;
    const int R = 2;
    const int S = 3;
    const int M = 0;

    [field: SerializeField] public GameObject Target { get; private set; }
    public Node TargetNode { get; private set; }
    public Node CurrentNode { get; private set; }
    [field: SerializeField, Range(10, 50)] private float Speed { get; set; } = 10.0f;
    [field: SerializeField, Range(0, 360)] private float Angle { get; set; } = 90;
    [field: SerializeField, Range(1, 10)] private float Scale { get; set; } = 1.25f;
    [field: SerializeField, Range(2, 360)] private int RayCount { get; set; }
    [field: SerializeField] private List<Vector3> Vertices { get; set; }

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

    // .. 코드 정리가 안된다..
    void FindWay()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity) || 
            hit.transform.name.Contains("Node_") ||
            !Target) return;

        hit.transform.gameObject.TryGetComponent(out MeshFilter meshFilter);

        Vector3[] verticesPoint = meshFilter.mesh.vertices;

        // .. 어떤 노드가 나랑 가장 가까운지 찾아주는 ..
        float pivotDistance = 1000000.0f;
        float endDistance = pivotDistance;

        Node pivotNode = null, endNode = null;

        List<Node> openList = new List<Node>(); // .. 다음 노드가 될 수 있는 후보 목록

        for (int i = 0; i < verticesPoint.Length; ++i)
        {
            if (verticesPoint[i].y >= transform.position.y + 0.05f ||
                verticesPoint[i].y + 0.05f <= transform.position.y) continue;
            #region GetWayPoint
            Matrix4x4[] matrix = new Matrix4x4[4];

            matrix[T] = Matrix.Translate(new Vector3(hit.transform.position.x, 0.0f, hit.transform.position.z));
            matrix[R] = Matrix.Rotate(hit.transform.eulerAngles);
            matrix[S] = Matrix.Scale(new Vector3(hit.transform.lossyScale.x, 0.0f, hit.transform.lossyScale.z) * Scale);
            matrix[M] = matrix[T] * matrix[R] * matrix[S];

            Vector3 wayPoint = matrix[M].MultiplyPoint(verticesPoint[i]);
            #endregion

            if (!Vertices.Contains(wayPoint))
            {
                Node node = new GameObject("Node_" + Vertices.Count.ToString()).AddComponent<Node>();

                node.gameObject.AddComponent<MyGizmo>();
                node.transform.position = wayPoint;

                float distance = CustomMath.GetDistance(node.transform.position.z, transform.position.z,
                                                        node.transform.position.x, transform.position.x);
                
                float targetDistance = CustomMath.GetDistance(node.transform.position.z, Target.transform.position.z,
                                                              node.transform.position.x, Target.transform.position.x);

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

                Vertices.Add(wayPoint);
            }
        }

        if (pivotNode)
            pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.red;

        if (endNode)
        {
            endNode.Next = new GameObject("EndNode").AddComponent<Node>();
            endNode.Next.transform.position = Target.transform.position;
        }

        // 조건 이동하는 도중에 충돌하는 물체가 없어야하고 이동할때 드는 비용이 가장 적은 노드를 찾기, 해당 노드와 목표로 하는 타겟노드와의 거리가 가장 적은가? 찾기
        while (!pivotNode.Equals(endNode))
        {
            float distance = 1000000.0f;
            float startTargetDistance = distance;
            int index = -1;

            for (int i = 0; i < openList.Count; ++i)
            {
                float intervalDistance = CustomMath.GetDistance(pivotNode.transform.position.z, openList[i].transform.position.z, 
                                                                pivotNode.transform.position.x, openList[i].transform.position.x);

                // 현재 pivot이 되는 노드가 다음 openList의 노드사이에 무언가 있는지 검사 뭔가 있다면? 해당 노드는 제외
                bool isCollision = Physics.Raycast(
                    pivotNode.transform.position, 
                    (openList[i].transform.position - pivotNode.transform.position).normalized,
                    out RaycastHit checkHit,
                    intervalDistance
                );

                float targetDistance = CustomMath.GetDistance(Target.transform.position.z, openList[i].transform.position.z,
                                                              Target.transform.position.x, openList[i].transform.position.x);

                if (!isCollision)
                {
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
            }

            if (index >= 0)
            {
                pivotNode.Next = openList[index];
                openList[index].Parent = pivotNode;
                pivotNode = openList[index];
                pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.green;
                openList.Remove(openList[index]);
            }
        }

        while (pivotNode.Parent)
            pivotNode = pivotNode.Parent;

        TargetNode = pivotNode;
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

    void OutMatrix(Matrix4x4 m)
    {
        Debug.Log("===================================================");
        Debug.Log(m.m00 + ", " + m.m01 + ", " + m.m02 + ", " + m.m03);
        Debug.Log(m.m10 + ", " + m.m11 + ", " + m.m12 + ", " + m.m13);
        Debug.Log(m.m20 + ", " + m.m21 + ", " + m.m22 + ", " + m.m23);
        Debug.Log(m.m30 + ", " + m.m31 + ", " + m.m32 + ", " + m.m33);
        Debug.Log("===================================================");
    }
    IEnumerator SetRotation()
    {
        float time;
        isMove = true;

        int count = Random.Range(2, 4) + 1;

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