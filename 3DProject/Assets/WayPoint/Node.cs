using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MyGizmo))]
public class Node : MonoBehaviour
{
    [field: SerializeField] public Node Next { get; set; }
    [field: SerializeField] public float Cost { get; set; }
    [field: SerializeField] public bool IsShort { get; set; }
    private void Awake()
    {
        IsShort = false;
        Cost = 0.0f;
        Next = null;
    }
}
