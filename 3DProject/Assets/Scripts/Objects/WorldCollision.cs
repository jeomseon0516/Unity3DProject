using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * .. 해당 컴포넌트는 중력이나 벽충돌에 관한 처리를 담당합니다. 
 * 유니티는 컴포넌트 패턴으로 구현되어 있다.
 * 컴포넌트 패턴은 하나의 클래스에 많은 기능이 집중되어 유지보수가 어려워지는걸 방지하기 위한 디자인 패턴이다.
 * Context 클래스의 기능이나 동작을 다시 클래스의 단위로 분리하여 동작을 수행하게끔 하는 패턴이다.
 * 상속 관계가 커지는 것도 마찬 가지이다. 그래서 유니티 또한 컴포넌트들이 GameObject를 상속받는 것이 아닌
 * GameObject가 Component List를 가지고 각 컴포넌트들의 동작을 수행하게끔 설계되어 있는 것이다. 
 */
public class WorldCollision : MonoBehaviour
{
    [SerializeField, Range(0.0f, 0.1f)] private float _weight;
    private int _layerNum;
    public float Height { get; private set; }
    public bool OnPlaneCollision { get; private set; }
    public bool IsCorrection { get; set; }

    private void Awake()
    {
        Height = VerticesTo.GetHeightFromVertices(gameObject); // .. 객체의 높이를 구해줌..
        _weight = 0.05f;

        _layerNum = LayerMask.NameToLayer("Plane");

        OnPlaneCollision = false;
        IsCorrection = true;
    }
    void Update()
    {
        float distance = Height * 0.5f;
        Vector3 pivotPoint = new Vector3(transform.position.x, transform.position.y + distance, transform.position.z);
        float weightDistance = distance + _weight;

        OnPlaneCollision = false;

        Debug.DrawRay(pivotPoint, Vector3.down * (IsCorrection ? weightDistance : distance), Color.blue);

        if (Physics.Raycast(pivotPoint, Vector3.down, out RaycastHit hit, weightDistance, 1 << _layerNum)) // .. Ray를 발사, 캐릭터가 바닥을 뚫었을때 보정
        {
            OnPlaneCollision = true;

            if (IsCorrection)
            {
                float yInterval = hit.point.y - transform.position.y;
                transform.position = new Vector3(transform.position.x, transform.position.y + yInterval, transform.position.z);
            }
        }
    }
}
