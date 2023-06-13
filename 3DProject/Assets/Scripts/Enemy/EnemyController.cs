using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public Node Target { get; private set; }
    [field: SerializeField, Range(10, 50)] private float Speed { get; set; } = 10.0f;
    [field: SerializeField, Range(0, 360)] private float Angle { get; set; } = 90;
    [field: SerializeField, Range(2, 360)] private int RayCount { get; set; }
    [field: SerializeField] private Material material { get; set; }

    [field: SerializeField] private List<Vector3> vertices { get; set; }

    private bool isMove;

    private void Awake()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GameObject parentNode = GameObject.Find("NAKZI");

        if (parentNode)
        {
            parentNode.transform.GetChild(0).TryGetComponent(out Node target);
            Target = target;
        }
    }
    void Start()
    {
        RayCount = 5;

        material.color = Color.red;

        isMove = false;
        Angle = 45;

        StartCoroutine(SetRotation());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity))
            {
                hit.transform.gameObject.TryGetComponent(out MeshFilter meshFilter);
                Vector3[] verticesPoint = meshFilter.mesh.vertices;

                for (int i = 0; i < verticesPoint.Length; ++i)
                    if (!vertices.Contains(verticesPoint[i]) && verticesPoint[i].y < transform.position.y)
                        vertices.Add(verticesPoint[i]);
            }

            for (int i = 0; i < vertices.Count; ++i)
            {
                GameObject obj = new GameObject(i.ToString());
                obj.transform.position = new Vector3(
                    vertices[i].x * hit.transform.lossyScale.x,
                    vertices[i].y,
                    vertices[i].z * hit.transform.lossyScale.z) + hit.transform.position;

                // obj.AddComponent<MyGizmo>();
            }
        }

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
        if (ReferenceEquals(Target.gameObject, other.gameObject))
        {
            StartCoroutine(SetRotation());
            Target = Target.Next;
        }
    }
    void Move()
    {
        if (!Target) return;

        Vector3 direction = (Target.transform.position - transform.position).normalized;

        if (isMove)
            transform.position = Vector3.Lerp(
                transform.position,
                Target.transform.position,
                0.016f);
        //transform.position += direction * Speed * Time.deltaTime;
    }

    IEnumerator SetRotation()
    {
        float time;
        isMove = false;

        int count = Random.Range(2, 4) + 1;

        for (int i = 0; i < count; ++i)
        {
            time = 0.0f;

            float radian = CustomMath.ConvertFromAngleToRadian(transform.eulerAngles.y + Random.Range(-Angle, Angle));

            while (time < 1.0f)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(new Vector3(Mathf.Sin(radian), 0.0f, Mathf.Cos(radian))),
                    0.016f);

                time += Time.deltaTime;
                yield return null;
            }
        }

        time = 0.0f;

        while (time < 1.0f)
        {
            if (Target)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation((Target.transform.position - transform.position).normalized),
                    0.016f);

                time += Time.deltaTime;
            }

            yield return null;
        }

        isMove = true;
    }


}