using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGizmo : MonoBehaviour
{
    public Color GizmoColor { get; set; }
    public float Radius { get; set; }
    public Vector3 Pivot { get; set; }

    private void Awake()
    {
        GizmoColor = Color.blue;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(Pivot, Radius);
    }
}
