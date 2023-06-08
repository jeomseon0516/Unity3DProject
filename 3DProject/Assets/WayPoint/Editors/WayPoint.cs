using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    private GameObject parent;
    void Start()
    {
        parent = GameObject.Find("NAKZI");
    }

    void Update()
    {
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            parent.transform.GetChild(i).TryGetComponent(out Node node);
            Debug.DrawLine(node.transform.position, node.Next.transform.position, Color.white);
        }
    }
}