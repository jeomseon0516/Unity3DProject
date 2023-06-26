using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR_WIN
[RequireComponent(typeof(AStarNodeDebug))]
#endif
public class AStar : MonoBehaviour
{
    List<Vector3> _nodes = new List<Vector3>();
    [field:SerializeField] public GameObject TargetObject { get; set; }

    void Start()
    {
        
    }

    void Update()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;


    }
}
