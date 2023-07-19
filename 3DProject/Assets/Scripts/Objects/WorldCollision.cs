using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 해당 컴포넌트는 중력이나 벽충돌에 관한 처리를 담당합니다.
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

        if (Physics.Raycast(pivotPoint, Vector3.down, out RaycastHit hit, weightDistance, 1 << _layerNum))
        {
            OnPlaneCollision = true;

            float yInterval = hit.point.y - transform.position.y;
            transform.position = new Vector3(transform.position.x, transform.position.y + yInterval, transform.position.z);
        }
        else
            OnPlaneCollision = false;
    }
}
