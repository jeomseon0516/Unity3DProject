using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public partial class EnemyController : MonoBehaviour
{
    [SerializeField, Range(10, 50)] private float _speed;
    [SerializeField, Range(0, 360)] private float _angle;

    private bool _isMove;

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
        _rayCount = 5;

        _isMove = true;
        _angle = 45;
        _scale = 1.45f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            findWay();

        move();
    }
    private void FixedUpdate()
    {
        float pivotRadian = CustomMath.ConvertFromAngleToRadian(transform.eulerAngles.y);
        float intervalAngle = _angle / _rayCount;
        float radian = -(CustomMath.ConvertFromAngleToRadian(_rayCount * 0.5f * intervalAngle));

        for (int i = 0; i < _rayCount + 1; ++i)
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

    private void move()
    {
        if (!TargetNode) return;

        Vector3 direction = (TargetNode.transform.position - transform.position).normalized;

        if (_isMove)
            transform.position += direction * _speed * Time.deltaTime;

        float distance = CustomMath.GetDistance(TargetNode.transform.position.z, transform.position.z,
                                                TargetNode.transform.position.x, transform.position.x);

        if (distance < 0.1f)
        {
            StartCoroutine(setRotation());
            TargetNode = TargetNode.Next ?? TargetNode;
        }
    }

    private IEnumerator setRotation()
    {
        float time;
        _isMove = true;

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

        _isMove = true;
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
    [SerializeField, Range(1, 10)] private float _scale;
    [SerializeField, Range(2, 360)] private int _rayCount;
    [SerializeField] private List<Vector3> _vertices;
    [field: SerializeField] public GameObject Target { get; private set; }
    public Node TargetNode { get; private set; }

    private void findWay()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity) ||
            hit.transform.name.Contains("Node_") ||
            !Target) return;

        _vertices.Clear();
        _vertices.AddRange(convertFromVerticesToNode(hit.transform.gameObject, _vertices, getVertices(hit.transform.gameObject)));

        List<Node> openList = makeNodes(_vertices, 0);

        List<GameObject> colList = new List<GameObject>();
        colList.Add(hit.transform.gameObject);

        Node startNode = new GameObject("StartNode").AddComponent<Node>();
        Node endNode = new GameObject("EndNode").AddComponent<Node>();
        Node pivotNode = startNode, shortNode = startNode;

        startNode.GetComponent<MyGizmo>().GizmoColor = Color.red;
        startNode.transform.position = transform.position;

        endNode.GetComponent<MyGizmo>().GizmoColor = Color.cyan;
        endNode.transform.position = Target.transform.position;

        TargetNode = pathFindingNodes(openList, new List<Node>(openList), startNode, shortNode, pivotNode, endNode, colList);
    }
    private List<Node> makeNodes(List<Vector3> vertices, int count)
    {
        List<Node> nodes = new List<Node>();

        foreach (Vector3 vertex in vertices)
        {
            Node node = new GameObject("Node_" + (nodes.Count + count).ToString()).AddComponent<Node>();

            node.transform.position = vertex;
            nodes.Add(node);
        }

        return nodes;
    }
    // 노드의 길을 찾은 후 StartNode 반환 재귀적으로 동작
    private Node pathFindingNodes(List<Node> oldList, List<Node> openList, Node startNode, Node shortNode, Node pivotNode, Node endNode, List<GameObject> colList)
    {
        float distance = Mathf.Infinity;
        int index = -1;

        for (int i = 0; i < openList.Count; ++i)
        {
            if (pivotNode.Equals(openList[i])) continue;

            float intervalDistance = CustomMath.GetDistance(pivotNode.transform.position.z, openList[i].transform.position.z,
                                                            pivotNode.transform.position.x, openList[i].transform.position.x);

            if (checkNextNodeCollision(colList, out RaycastHit checkHit, pivotNode, openList[i], intervalDistance))
            {
                if (colList.Contains(checkHit.transform.gameObject)) continue;

                List<Vector3> newVertices = convertFromVerticesToNode(checkHit.transform.gameObject, _vertices, getVertices(checkHit.transform.gameObject));
                _vertices.AddRange(newVertices);

                openList.Clear();
                oldList.AddRange(makeNodes(newVertices, oldList.Count));
                openList.AddRange(oldList);

                pivotNode = startNode;
                shortNode = startNode;

                index = -1;

                colList.Add(checkHit.transform.gameObject);

                break;
            }

            if (distance > intervalDistance)
            {
                distance = intervalDistance;
                index = i;
            }
        }

        if (index >= 0)
        {
            pivotNode.Next = openList[index];
            openList[index].Cost = pivotNode.Cost + distance;
            pivotNode = openList[index];

            // .. 피벗노드와 숏노드의 거리를 구한다.
            float shortDistance = CustomMath.GetDistance(pivotNode.transform.position.z, shortNode.transform.position.z,
                                                         pivotNode.transform.position.x, shortNode.transform.position.x);

            // .. 중간에 충돌하는 오브젝트가 있는지 확인한다. 충돌하는 오브젝트가 있다면 shortNode를 갱신한다.
            if (Physics.Raycast(shortNode.transform.position,
               (pivotNode.transform.position - shortNode.transform.position).normalized,
                out RaycastHit hit, shortDistance) && !hit.transform.name.Contains("Node"))
            {
                shortNode = pivotNode;
            }
            else
            {
                if (shortDistance < pivotNode.Cost - shortNode.Cost)
                    shortNode.Next = pivotNode;
            }

            pivotNode.GetComponent<MyGizmo>().GizmoColor = Color.green;
            openList.Remove(openList[index]);
        }

        float targetDistance = CustomMath.GetDistance(pivotNode.transform.position.z, endNode.transform.position.z,
                                                      pivotNode.transform.position.x, endNode.transform.position.x);

        bool condition = Physics.Raycast(pivotNode.transform.position, (endNode.transform.position - pivotNode.transform.position).normalized,
            out RaycastHit endHit,
            targetDistance);

        if (condition && !endHit.transform.name.Contains("Node") && !endHit.transform.name.Contains(Target.transform.name))
            pathFindingNodes(oldList, openList, startNode, shortNode, pivotNode, endNode, colList);
        else
            pivotNode.Next = endNode;

        return startNode;
    }
    private bool checkNextNodeCollision(List<GameObject> colList, out RaycastHit hit, Node pivotNode, Node checkNode, float distance)
    {
        // .. 현재 pivot이 되는 노드가 검사할 openList의 노드사이에 무언가 있는지 검사 뭔가 있다면? 해당 노드는 제외
        bool isCollision = Physics.Raycast(
                pivotNode.transform.position,
                (checkNode.transform.position - pivotNode.transform.position).normalized,
                out hit,
                distance
            );
        // .. 진행동선중에 노드가 아닌 충돌체가 존재한다면?
        return isCollision && !hit.transform.name.Contains("Node");
    }

    // 메쉬에서 받아온 Vertex를 행렬을 이용하여 오브젝트 사이즈로 변환
    private Vector3 convertFromVertexToVector(GameObject obj, Vector3 vertexPoint, float scale)
    {
        Matrix4x4[] matrix = new Matrix4x4[4];

        matrix[T] = Matrix.Translate(new Vector3(obj.transform.position.x, 0.0f, obj.transform.position.z));
        matrix[R] = Matrix.Rotate(obj.transform.eulerAngles);
        matrix[S] = Matrix.Scale(new Vector3(obj.transform.lossyScale.x, 0.0f, obj.transform.lossyScale.z) * scale);
        matrix[M] = matrix[T] * matrix[R] * matrix[S];

        return matrix[M].MultiplyPoint(vertexPoint);
    }
    // 메쉬에서 받아온 Vertices를 오브젝트 사이즈로 변환 
    private List<Vector3> convertFromVerticesToNode(GameObject obj, List<Vector3> vertices, List<Vector3> origin)
    {
        List<Vector3> newVertices = new List<Vector3>();

        foreach (Vector3 vertex in origin)
        {
            if (vertex.y <= 0.0f) continue;

            Vector3 wayPoint = convertFromVertexToVector(obj, vertex, _scale);

            if (!vertices.Contains(wayPoint) && !newVertices.Contains(wayPoint))
                newVertices.Add(wayPoint);
        }

        return newVertices;
    }

    // 특정 오브젝트의 하위 오브젝트에 있는 모든 메쉬를 받아오는 함수
    private List<Vector3> getVertices(GameObject hitObject)
    {
        List<Vector3> vertexList = new List<Vector3>();

        for (int i = 0; i < hitObject.transform.childCount; ++i)
            vertexList.AddRange(getVertices(hitObject.transform.GetChild(i).gameObject));

        if (!hitObject.transform.gameObject.TryGetComponent(out MeshFilter meshFilter))
            return vertexList;

        vertexList.AddRange(meshFilter.mesh.vertices);

        return vertexList;
    }
    // Debug
    private void outMatrix(Matrix4x4 m)
    {
        Debug.Log("===================================================");
        Debug.Log(m.m00 + ", " + m.m01 + ", " + m.m02 + ", " + m.m03);
        Debug.Log(m.m10 + ", " + m.m11 + ", " + m.m12 + ", " + m.m13);
        Debug.Log(m.m20 + ", " + m.m21 + ", " + m.m22 + ", " + m.m23);
        Debug.Log(m.m30 + ", " + m.m31 + ", " + m.m32 + ", " + m.m33);
        Debug.Log("===================================================");
    }
}