using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    [field:SerializeField, Range(0, 180)] public float Angle { get; set; }
    [field: SerializeField, Range(0, 1)] public float Dis { get; set; }

    private Vector3 temp;
    private Vector3 dest;

    private void Start()
    {
        Angle = 0.0f;

        temp = new Vector3(0.0f, 0.0f, 0.0f);
        dest = new Vector3(10.0f, 0.0f, 0.0f);

        StartCoroutine(SetMove(temp, dest));
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(0.0f, Angle, 0.0f);

        /*
        Quaternion rotation = transform.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation.);

        transform.rotation = rotation;
        */

        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(10.0f, 0.0f, 0.0f),
            0.016f);

        Debug.DrawRay(transform.position, 
            new Vector3(
                Mathf.Sin(Angle * Mathf.Deg2Rad), 
                0.0f, 
                Mathf.Cos(Angle * Mathf.Deg2Rad)
                ) * 2.5f, 
            Color.green);

        Debug.DrawRay(transform.position,
            new Vector3(
                Mathf.Sin(-Angle * Mathf.Deg2Rad),
                0.0f,
                Mathf.Cos(Angle * Mathf.Deg2Rad)
                ) * 2.5f,
            Color.green);
    }

    IEnumerator SetMove(Vector3 point, Vector3 next)
    {
        float time = 0.0f;

        while (time < 1.0f)
        {
            transform.position = Vector3.Lerp(
                point,
                next,
                time);

            time += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(SetMove(next, point));
    }
}
