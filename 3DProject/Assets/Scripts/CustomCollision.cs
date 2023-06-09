using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollision : MonoBehaviour
{
    [field: SerializeField]
    public Vector3[] Vertics { get; private set; }

    void Start()
    {
        Vertics[0] = transform.position;

        Vertics = new Vector3[15];

        for (int i = 1; i < Vertics.Length; ++i)
            Vertics[i] = new Vector3(Vertics[i - 1].x + Random.Range(10, 30), Random.Range(5, 50), 0);
    }

    void Update()
    {
        for (int i = 0; i < Vertics.Length - 1; ++i)
            Debug.DrawLine(Vertics[i], Vertics[i + 1]);
    }
}
