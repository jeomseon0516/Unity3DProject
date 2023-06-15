using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Node : MonoBehaviour
{
    [field:SerializeField]
    public Node Next { get; set; }
    public Node Parent { get; set; }
    public float Cost { get; set; }
    private void Awake()
    {
        Parent = null;
        GetComponent<SphereCollider>().isTrigger = true;
    }
    private void Start()
    {
        Cost = 0.0f;
    }
}
