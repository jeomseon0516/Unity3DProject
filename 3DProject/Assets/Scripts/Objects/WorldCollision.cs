using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCollision : MonoBehaviour
{
    [SerializeField, Range(0.0f, 0.1f)] private float _weight;
    private float _height;
    int _layerNum;
    public bool OnPlaneCollision { get; private set; }

    private void Awake()
    {
        _height = VerticesTo.GetHeightFromVertices(gameObject);
        _weight = 0.05f;

        _layerNum = LayerMask.NameToLayer("Plane");

        OnPlaneCollision = false;
    }
    void Update()
    {
        float distance = _height * 0.5f;

        Vector3 pivotPoint = new Vector3(transform.position.x, transform.position.y + distance, transform.position.z);

        float weightDistance = distance + _weight; 

        Debug.DrawRay(pivotPoint, Vector3.down * weightDistance, Color.blue);

        if (Physics.Raycast(pivotPoint, Vector3.down, out RaycastHit hit, weightDistance, _layerNum))
        {
            float yInterval = hit.point.y - pivotPoint.y;
        }
    }
}
