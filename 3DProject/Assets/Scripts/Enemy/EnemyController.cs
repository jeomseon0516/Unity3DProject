using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public Node Target { get; set; }
    [field: SerializeField, Range(10, 50)]
    private float Speed { get; set; } = 10.0f;

    [field:SerializeField, Range(0, 360)]
    private float Angle { get; set; } = 90;

    private Vector3 leftRay, rightRay; // .. 더듬이 ..

    private void Awake()
    {
        GetComponent<Rigidbody>().useGravity = false;
        Target = GameObject.Find("NAKZI").transform.GetChild(0).GetComponent<Node>();
    }
    void Start()
    {
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

            Vector3 leftPoint  = (transform.position + leftRay);
            Vector3 rightPoint = (transform.position + rightRay); 

            float leftRadian  = CustomMath.GetToConvertRotationToRadian(leftPoint.x, transform.position.x,
                                                                        leftPoint.z, transform.position.z, 
                                                                        transform.eulerAngles.y);

            float rightRadian = CustomMath.GetToConvertRotationToRadian(rightPoint.x, transform.position.x,
                                                                        rightPoint.z, transform.position.z,
                                                                        transform.eulerAngles.y);

            Vector3 leftDirection  = new Vector3(Mathf.Sin(leftRadian),  0, Mathf.Cos(leftRadian));
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ReferenceEquals(Target.gameObject, other.gameObject))
            Target = Target.Next;
    }
}
