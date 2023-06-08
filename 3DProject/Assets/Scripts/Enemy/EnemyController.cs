using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public Node Target { get; set; }
    private float Speed { get; set; }

    [field: SerializeField, Range(0, 360)]
    private float Angle { get; set; } = 90;

    Vector3 leftRay, rightRay;

    private void Awake()
    {
        GetComponent<Rigidbody>().useGravity = false;
        Target = GameObject.Find("NAKZI").transform.GetChild(0).GetComponent<Node>();

    }
    void Start()
    {
        Speed = 5.0f;

        float x = 5.0f;
        float z = 5.0f;

        leftRay  = new Vector3(-x, 0, z);
        rightRay = new Vector3(x, 0, z);
    }

    void Update()
    {
        if (Target)
        {
            Vector3 direction = (Target.transform.position - transform.position).normalized;

            transform.position += direction * Speed * Time.deltaTime;
            transform.LookAt(Target.transform);

            float radian = CustomMath.ConvertFromAngleToRadian(Angle);
            Vector3 rotateValue = new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));

            Vector3 leftDirection  = transform.position + leftRay  + rotateValue * 5.0f;
            Vector3 rightDirection = transform.position + rightRay + rotateValue * 5.0f;

            // leftDirection  = new Vector3(leftDirection.x  * rotateValue.x, 0, leftDirection.z  * rotateValue.z);
            // rightDirection = new Vector3(rightDirection.x * rotateValue.x, 0, rightDirection.z * rotateValue.z);

            Debug.DrawRay(transform.position, leftDirection, Color.red);
            if (Physics.Raycast(transform.position, leftDirection, out RaycastHit hit, 5.0f))
            {

            }

            Debug.DrawRay(transform.position, rightDirection, Color.red);
            if (Physics.Raycast(transform.position, rightDirection, out hit, 5.0f))
            {

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ReferenceEquals(Target.gameObject, other.gameObject))
            Target = Target.Next;
    }
}
