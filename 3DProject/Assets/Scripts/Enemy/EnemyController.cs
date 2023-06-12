using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public Node Target { get; set; }
    [field:SerializeField, Range(10, 50)] private float Speed { get; set; } = 10.0f;
    [field:SerializeField, Range(0, 360)] private float Angle { get; set; } = 90;

    private Vector3 leftRay, rightRay; // .. 더듬이 ..

    private bool isMove;

    private void Awake()
    {
        GetComponent<Rigidbody>().useGravity = false;
        Target = GameObject.Find("NAKZI").transform.GetChild(0).GetComponent<Node>();
    }
    void Start()
    {
        float x = 5.0f;
        float z = 5.0f;

        isMove = false;

        leftRay  = new Vector3(-x, 0, z);
        rightRay = new Vector3(x, 0, z);

        StartCoroutine(SetRotation());
    }

    void Update()
    {
        if (!Target) return;

        Vector3 direction = (Target.transform.position - transform.position).normalized;

        if (isMove)
            transform.position += direction * Speed * Time.deltaTime;
    }

    IEnumerator SetRotation()
    {
        float time;
        isMove = false;

        int count = Random.Range(2, 4) + 1;

        for (int i = 0; i < count; ++i)
        {
            time = 0.0f;

            Vector3 lookPosition = transform.position + new Vector3(Random.Range(-30, 30), 0.0f, Random.Range(-30, 30));

            while (time < 1.0f)
            {
                transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation((lookPosition - transform.position).normalized),
                0.016f);

                time += Time.deltaTime;
                yield return null;
            }
        }

        time = 0.0f;

        while (time < 1.0f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation((Target.transform.position - transform.position).normalized),
                0.016f);

            time += Time.deltaTime;
            yield return null;
        }

        isMove = true;
    }

    private void FixedUpdate()
    {
        Vector3 leftPoint  = transform.position + leftRay;
        Vector3 rightPoint = transform.position + rightRay;

        float leftRadian = CustomMath.GetToConvertRotationToRadian(leftPoint.x, transform.position.x,
                                                                    leftPoint.z, transform.position.z,
                                                                    transform.eulerAngles.y);

        float rightRadian = CustomMath.GetToConvertRotationToRadian(rightPoint.x, transform.position.x,
                                                                    rightPoint.z, transform.position.z,
                                                                    transform.eulerAngles.y);

        Vector3 leftDirection  = new Vector3(Mathf.Sin(leftRadian), 0, Mathf.Cos(leftRadian));
        Vector3 rightDirection = new Vector3(Mathf.Sin(rightRadian), 0, Mathf.Cos(rightRadian));

        Debug.DrawRay(transform.position, leftDirection * 5.0f, Color.red);
        if (Physics.Raycast(transform.position, leftDirection, out RaycastHit hit, 5.0f))
        {

        }

        Debug.DrawRay(transform.position, rightDirection * 5.0f, Color.red);
        if (Physics.Raycast(transform.position, rightDirection, out hit, 5.0f))
        {

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
}
