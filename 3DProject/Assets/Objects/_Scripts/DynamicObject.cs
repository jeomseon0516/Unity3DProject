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

[RequireComponent(typeof(MyGizmo))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class DynamicObject : MonoBehaviour
{
    #region MField
    /*
     * -----------------------------------------------------------------------------------------
     *                                      MemberField
     * -----------------------------------------------------------------------------------------
     */
    private readonly float CHECK_GROUND_DISTANCE = 0.01f; // .. 지면인식 허용 거리 내리막길에서 튀는 현상 방지
    private readonly float CHECK_MAX_GROUND_INTERVAL = 0.0001f; // .. 지면 체크를 할때 내보정값.
    private readonly float SPHERE_MAX_DISTANCE = 2.0f; // .. Sphere 캐스팅할 거리
    private readonly float HORIZONTAL_SLOPE_ANGLE_RATIO = 1 / 90f; // .. 90도를 1의 비율로 나타내었을때의 값
    public class Components
    {
        public Rigidbody rigidbody;
        public MyGizmo sphereGizmo;
        public MyGizmo groundCollisionGizmo;
        public CapsuleCollider capsuleCollider;
    }
    public class MoveOption
    {
        public Vector3 lookAt;
        public Vector3 direction;
        public Vector3 horizontalVelocity;
        public float groundInterval; // .. 지면과의 거리
        public float speed;
    }
    public class CollisionState
    {
        public bool isGrounded;
        public bool isForwardBlocked;
    }

    private float m_fixedDeltaTime;
    private float m_castRadius; // .. SphereCast시 원점이랑 오버랩발생시 충돌이 일어나지 않으므로 캡슐 Radius보다 조금더 작은 값으로 초기화.
    private float m_castRadiusDiff; // .. ground와의 distance 계산시 Radius의 오차 범위를 계산하기 위해 저장 
    private float m_jumpingPower;
    private int m_layerNum;

    private MoveOption m_moveOption = new MoveOption();
    private Components m_components = new Components();
    private CollisionState m_collisionState = new CollisionState();
    public Components Com => m_components;
    public MoveOption MOption => m_moveOption;
    public CollisionState CState => m_collisionState;
    [field: SerializeField, Range(0.0f, Constants.GRAVITY)] public float Gravity { get; set; }
    public float Height { get; private set; }
    public bool IsJump { get; set; }
    public Vector3 CapsuleBottomCenterPoint => m_components.rigidbody.position + new Vector3(0.0f, m_components.capsuleCollider.radius, 0.0f);
    [field: SerializeField, Range(0.0f, 8.0f)] public float Speed { get; set; }
    #endregion

    /*
     * .. 내리막길 Slope처리로 인해 점프시 지속적으로 땅에 붙는 현상 발생.
     * 로직의 순서 문제.
     * 점프 신호나 추락인지 판단 -> 점프나 공중일시 중력값을 받아옴 -> 이동 처리 -> 지형이냐 공중이냐에 따른 Slope 보정 -> 최종적인 Velocity 계산 -> 이동  
     */
    private void Awake()
    {
        // .. 이니셜라이징
        initRigidbody();
        initMoveOption();
        initCapsuleCollider();
        initCollisionState();
#if UNITY_EDITOR_WIN
        initGizmos();
#endif
        Gravity = 0f;
        m_layerNum = LayerMask.NameToLayer("Plane");
        m_jumpingPower = 10.0f;
        IsJump = false;
    }
    private void Start()
    {
        m_fixedDeltaTime = Time.fixedDeltaTime;
    }
    private void Update()
    {
        transform.rotation = 
            Quaternion.Lerp(
            transform.rotation, 
            Quaternion.LookRotation(m_moveOption.lookAt), 
            20.0f * Time.deltaTime
            );
    }
    private void FixedUpdate()
    {
        moveObject(m_moveOption.direction, m_moveOption.speed);
    }
    #region Initializer
    private void initMoveOption()
    {
        m_moveOption.lookAt = Vector3.zero;
        m_moveOption.direction = Vector3.zero;
        m_moveOption.horizontalVelocity = Vector3.zero;
        m_moveOption.speed = 0f;
        m_moveOption.groundInterval = 0f;
    }
    private void initCollisionState()
    {
        m_collisionState.isGrounded = false;
        m_collisionState.isForwardBlocked = false;
    }
    private void initCapsuleCollider()
    {
        if (!TryGetComponent(out m_components.capsuleCollider)) return;

        CapsuleCollider capsuleCollider = m_components.capsuleCollider;
        Height = VerticesTo.GetHeightFromVertices(gameObject); // .. 객체의 높이를 구해줌..
        capsuleCollider.height = Height;
        capsuleCollider.center = Vector3.up * Height * 0.5f;
        capsuleCollider.radius = 0.25f;
        m_castRadius = m_components.capsuleCollider.radius * 0.9f;
        m_castRadiusDiff = m_components.capsuleCollider.radius - m_castRadius;
    }
    private void initRigidbody()
    {
        if (!TryGetComponent(out m_components.rigidbody)) return;

        m_components.rigidbody.useGravity = false;
        m_components.rigidbody.freezeRotation = true;
    }
    private void initGizmos()
    {
        if (!TryGetComponent(out m_components.sphereGizmo)) return;

        m_components.sphereGizmo.Radius = m_castRadius;
        m_components.sphereGizmo.Pivot = Vector3.zero;
        m_components.sphereGizmo.GizmoColor = Color.blue;
        m_components.groundCollisionGizmo = gameObject.AddComponent<MyGizmo>();
        m_components.groundCollisionGizmo.Radius = 0.1f;
        m_components.groundCollisionGizmo.Pivot = Vector3.zero;
    }
    #endregion
    private void moveObject(Vector3 direction, float speed)
    {
        m_fixedDeltaTime = Time.fixedDeltaTime;
        m_moveOption.direction = direction;

        float max = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.z));
        Vector3 velocity = transform.forward * speed * max;

        m_moveOption.horizontalVelocity = getHorizontalVelocityFromGroundNormal(
                velocity, // .. 현재 내가 진행할 방향의 속력으로..
                transform.forward, // .. 현재 내가 진행할 방향
                CapsuleBottomCenterPoint, // .. 캡슐 캐스팅을 시작할 지점
                SPHERE_MAX_DISTANCE // .. 
                );

        if (m_collisionState.isGrounded && !IsJump)
            Gravity = 0f;
        else
        {
            IsJump = false;
            Gravity += Constants.GRAVITY * m_fixedDeltaTime;
        }

        m_components.sphereGizmo.Pivot  = m_components.rigidbody.position + new Vector3(0.0f, m_castRadius, 0.0f);
        m_components.rigidbody.velocity = m_moveOption.horizontalVelocity + Vector3.up * Gravity;
    }
    // .. 해당 함수는 지면을 체크합니다. 지면의 경사각을 체크하여 투영된 방향을 구해줍니다.

    // .. 1. 이동할 위치에 레이를 쏜다
    // .. 2. 현재 위치와 이동할 위치에 찍은 점과 점사이의 각도를 구한다.
    // .. 3. 
    private Vector3 getHorizontalVelocityFromGroundNormal(Vector3 velocity, Vector3 direction, Vector3 pivotPoint, float distance)
    {
        m_collisionState.isGrounded = false;

        bool cast = Physics.SphereCast(
            pivotPoint,
            m_castRadius,
            Vector3.down,
            out RaycastHit hit,
            distance,
            1 << m_layerNum
            );

        Debug.DrawRay(pivotPoint, Vector3.down * distance, Color.blue);

        if (cast)
        {
            m_components.groundCollisionGizmo.Pivot = hit.point;
            m_components.groundCollisionGizmo.GizmoColor = new Color(0, 1, 1, 0.5f);
            m_moveOption.groundInterval = Mathf.Max(hit.distance - m_castRadiusDiff - CHECK_GROUND_DISTANCE, -10f);
            m_collisionState.isGrounded = hit.point.y + CHECK_MAX_GROUND_INTERVAL >= transform.position.y;

            // .. angle값 벡터를 내적하여 각도로 변환한 것과 같다.
            /*
             *  .. 아래와 같음 (내적하여 각도를 구하는 방법)
             *  float dot = Vector3.Dot(hit.normal, Vector3.up); 코사인 값
             *  float radian = Mathf.Acos(dot);
             *  float angle = radian * Mathf.Rad2Deg;
             */

            Vector3 alignmentVector3 = Vector3.Cross(hit.normal, )

            float groundSlopeAngle  = Vector3.Angle(hit.normal, Vector3.up);
            float forwardSlopeAngle = Mathf.Abs(Vector3.Angle(hit.normal, direction) - 90f);
            // .. 외적한 값
            Vector3 groundCross = Vector3.Cross(hit.normal, Vector3.up);
            // .. 지면의 경사각 만큼 이동속도 감소
            velocity *= HORIZONTAL_SLOPE_ANGLE_RATIO * -forwardSlopeAngle + 1f;
            // .. 외적한 값을 기준으로 속도를 회전
            velocity = Quaternion.AngleAxis(-groundSlopeAngle, groundCross) * velocity;
        }

        return velocity;
    }
    public void MoveObject(Vector3 direction, Vector3 lookAt, float speed)
    {
        m_moveOption.direction = direction;
        m_moveOption.lookAt = lookAt;
        m_moveOption.speed = speed;
    }
    public void SetJump()
    {
        // m_worldCollision.Gravity = m_jumpingPower;
        // m_worldCollision.IsJump = true;
    }
}
