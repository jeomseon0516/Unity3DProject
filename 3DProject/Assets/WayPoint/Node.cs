using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Node : MonoBehaviour
{
    [field:SerializeField]
    public Node Next { get; set; }
    private void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }
    void Start()
    {
    }
    
    void Update()
    {
        
    }
}
