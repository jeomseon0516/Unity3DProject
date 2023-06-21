using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGizmo : MonoBehaviour
{
    public Color GizmoColor { get; set; }

    private void Awake()
    {
        GizmoColor = Color.blue;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
