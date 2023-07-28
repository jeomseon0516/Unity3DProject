using System;
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
[RequireComponent(typeof(Rigidbody))]
public class WorldCollision : MonoBehaviour
{
    public class Components
    {
        [HideInInspector] public Rigidbody rigidBody;
    }
    [Serializable]
    public class CollisionOption
    {
        [Range(0.0f, Constants.GRAVITY)] public float gravity = 0.0f;
    }
    [Serializable]
    public class CollisionState
    {
        public bool isFall;
        public bool isForwardBlocked;
    }

    private CollisionOption _collisionOption = new CollisionOption();
    private CollisionState _collisionState = new CollisionState();
    private Components _components = new Components();

    private Components Com => _components;
    private CollisionOption COption => _collisionOption;
    private CollisionState CState => _collisionState;

    private int _layerNum;
    private float _castRadius;
    private float _fixedDeltaTime;
    public float Height { get; private set; }
    public float Gravity { get => COption.gravity; }
    public bool IsFall { get => CState.isFall; }


    private void Awake()
    {
        _fixedDeltaTime = Time.fixedDeltaTime;

        Com.rigidBody = GetComponent<Rigidbody>();
        Height = VerticesTo.GetHeightFromVertices(gameObject); // .. 객체의 높이를 구해줌..
        COption.gravity = 0.0f;
        CState.isFall = false;

        _layerNum = LayerMask.NameToLayer("Plane");
        Com.rigidBody.useGravity = false;
        _weight = 0.05f;
    }
    private void Start()
    {
    }
    void FixedUpdate()
    {
        float distance = Height * 0.5f;
        Vector3 pivotPoint = new Vector3(_rigidbody.position.x, _rigidbody.position.y + distance, _rigidbody.position.z);
        float weightDistance = distance + _weight;

        checkGround(pivotPoint, weightDistance);
        fallObject();
    }
    private void initRigidbody()
    {
        if (TryGetComponent(out Com.rigidBody))
        {
            Com.rigidBody.useGravity = false;
            Com.rigidBody.freezeRotation = true;
        }
    }
    private void checkGround(Vector3 pivotPoint, float distance)
    {
        bool cast = Physics.SphereCast(pivotPoint, Vector3.down, out RaycastHit hit, distance, 1 << _layerNum);
        CState.isFall = true;

        if (cast) // .. Ray를 발사, 캐릭터가 바닥을 뚫었을때 보정
        {
            IsFall = false;

            float yInterval = hit.point.y - transform.position.y;
            _rigidbody.MovePosition(_rigidbody.position + new Vector3(0.0f, yInterval, 0.0f));
        }

    }
    private void fallObject()
    {
        if (!IsFall)
            Gravity = 0.0f;
        else
            Gravity -= Constants.GRAVITY * Time.deltaTime;
    }
}
