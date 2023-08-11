using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDynamicObject
{
    public void initDynamicObject() {}
}

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
     *                               MemberField
     * -----------------------------------------------------------------------------------------
     */

    private readonly float CHECK_GROUND_DISTANCE = 0.01f; // .. 지면인식 허용 거리 내리막길에서 튀는 현상 방지
    private readonly float CHECK_MAX_GROUND_INTERVAL = 0.0001f;
    private readonly float SPHERE_MAX_DISTANCE = 2.0f; // .. Sphere 캐스팅할 거리
    private readonly float HORIZONTAL_SLOPE_ANGLE_RATIO = 1 / 90f;

    public class Components
    {
        [HideInInspector] public Rigidbody rBody;
        [HideInInspector] public MyGizmo sphereGizmo;
        [HideInInspector] public MyGizmo groundCollisionGizmo;
        [HideInInspector] public CapsuleCollider capsuleCollider;
    }
    public class MoveOption
    {
        public Vector3 direction;
        public Vector3 horizontalVelocity;
        public Vector3 lookAt;
        public float groundInterval; // .. 지면과의 거리
    }
    public class CollisionState
    {
        public bool isGrounded;
        public bool isForwardBlocked;
    }

    private float m_fixedDeltaTime;
    private float m_castRadius; // .. SphereCast시 원점이랑 오버랩발생시 충돌이 일어나지 않으므로 캡슐 Radius보다 조금더 작은 값으로 초기화.
    private float m_castRadiusDiff; // .. ground와의 distance 계산시 Radius의 오차 범위를 계산하기 위해 저장 
    private int m_layerNum;

    private Components m_components = new Components();
    private MoveOption m_moveOption = new MoveOption();
    private CollisionState m_collisionState = new CollisionState();
    public Components Com => m_components;
    public CollisionState CState => m_collisionState;
    public MoveOption MOption => m_moveOption;
    [field:SerializeField, Range(0.0f, Constants.GRAVITY)] public float Gravity { get; set; }
    [field:SerializeField, Range(0.0f, 8.0f)] public float Speed { get; set; }
    public bool IsGrounded => CState.isGrounded;
    public float GroundInterval => m_moveOption.groundInterval;
    public float Height { get; private set; }
    public Vector3 CapsuleBottomCenterPoint => m_components.rBody.position + new Vector3(0.0f, m_components.capsuleCollider.radius, 0.0f);

    public Vector3 Direction 
    { 
        get => m_moveOption.direction; 
        set => m_moveOption.direction = value; 
    }
    public Vector3 LookAt
    {
        get => m_moveOption.lookAt;
        set => m_moveOption.lookAt = value;
    }
    #endregion

    /* .. 현재 문제점.. 점프와 지형간의 상호작용이 서로 충돌을 일으켜 제대로 동작이 되지 않음
     * 방식의 문제임 sphereCast로 지형이 있다는 걸 확인하면 발바닥이 땅을 뚫은 만큼 보정 시키므로..
     * 점프와의 충돌을 방지하려면 ..
     * 다음 프레임에 이동할 방향과 힘을 구해야함..
     * 1. 미리 내가 이동할 위치의 포지션을 구해 경사각을 구한다. (외적으로 구한 방향 * 내적)
     * 2. 이동이 가능한 경사인지 판별한다..
     * 3. 
     */
    private void Awake()
    {

#if UNITY_EDITOR_WIN
        initGizmos();
#endif
        // initComponents();
        initRigidbody();
        initCapsuleCollider();

        m_fixedDeltaTime = Time.fixedDeltaTime;
        m_castRadius = m_components.capsuleCollider.radius * 0.9f;
        m_castRadiusDiff = m_components.capsuleCollider.radius - m_castRadius;

        Gravity = 0f;

        m_collisionState.isGrounded = false;
        m_collisionState.isForwardBlocked = false;

        m_layerNum = LayerMask.NameToLayer("Plane");
    }
    private void Start()
    {
        Speed = 10.0f;
        m_moveOption.direction = new Vector3(0.0f, 0.0f, 0.0f);
        m_moveOption.lookAt = m_moveOption.direction;
    }
    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_moveOption.lookAt), 20.0f * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        float max = Mathf.Max(Mathf.Abs(m_moveOption.direction.x), Mathf.Abs(m_moveOption.direction.z));

        m_moveOption.horizontalVelocity = transform.forward * Speed * max;
        Vector3 direction = getDirectionFromGroundNormal(transform.forward, CapsuleBottomCenterPoint, SPHERE_MAX_DISTANCE);
        fallObject();

        m_components.rBody.velocity = m_moveOption.horizontalVelocity + Vector3.up * Gravity;
        m_components.sphereGizmo.Pivot = transform.position + new Vector3(0.0f, m_castRadius, 0.0f);
    }
    private void LateUpdate()
    {
    }
    private void initComponents()
    {
    }
    private void initCapsuleCollider()
    {
        if (!TryGetComponent(out m_components.capsuleCollider)) return;

        CapsuleCollider capsuleCollider = m_components.capsuleCollider;
        
        Height = VerticesTo.GetHeightFromVertices(gameObject); // .. 객체의 높이를 구해줌..

        capsuleCollider.height = Height;
        capsuleCollider.center = Vector3.up * Height * 0.5f;
        capsuleCollider.radius = 0.25f;
    }
    private void initRigidbody()
    {
        if (!TryGetComponent(out m_components.rBody)) return;

        m_components.rBody.useGravity = false;
        m_components.rBody.freezeRotation = true;
    }
    private void initGizmos()
    {
        if (!TryGetComponent(out Com.sphereGizmo)) return;

        m_components.sphereGizmo.Radius = m_castRadius;
        m_components.sphereGizmo.Pivot = Vector3.zero;

        m_components.groundCollisionGizmo = gameObject.AddComponent<MyGizmo>();
        m_components.groundCollisionGizmo.Radius = 0.1f;
        m_components.groundCollisionGizmo.Pivot = Vector3.zero;
    }
    // .. 해당 함수는 지면을 체크합니다. 지면의 경사각을 체크하여 투영된 방향을 구해줍니다.
    private Vector3 getDirectionFromGroundNormal(Vector3 direction, Vector3 pivotPoint, float distance)
    {
        bool cast = Physics.SphereCast(
            pivotPoint, 
            m_castRadius, 
            Vector3.down, 
            out RaycastHit hit,
            distance, 
            1 << m_layerNum
            );

        Debug.DrawRay(pivotPoint, Vector3.down * distance, Color.blue);

        CState.isGrounded = false;

        if (cast) // .. Ray를 발사, 캐릭터가 바닥을 뚫었을때 보정
        {
            m_moveOption.groundInterval = pivotPoint.y - hit.point.y;

            m_components.groundCollisionGizmo.Pivot = hit.point;
            m_components.groundCollisionGizmo.GizmoColor = new Color(0, 1, 1, 0.5f);

            m_moveOption.groundInterval = Mathf.Max(hit.distance - m_castRadiusDiff - CHECK_GROUND_DISTANCE, -10f);

            CState.isGrounded = m_moveOption.groundInterval <= CHECK_MAX_GROUND_INTERVAL;

            // .. 외적한 값
            float groundSlopeAngle  = Vector3.Angle(hit.normal, Vector3.up);
            float forwardSlopeAngle = Mathf.Abs(Vector3.Angle(hit.normal, direction) - 90f);
            Vector3 groundCross    = Vector3.Cross(hit.normal, Vector3.up);

            m_moveOption.horizontalVelocity *= HORIZONTAL_SLOPE_ANGLE_RATIO * -forwardSlopeAngle + 1f;
            m_moveOption.horizontalVelocity = Quaternion.AngleAxis(-groundSlopeAngle, groundCross) * m_moveOption.horizontalVelocity;
            // print(Vector3.Cross(hit.normal, Vector3.up));
            // Vector3 correctionDirection = Vector3.ProjectOnPlane(direction, hit.normal);
            // direction = correctionDirection;

            // Vector3 correctionPoint = m_components.rBody.position + projectDirection * Speed * m_fixedDeltaTime;

            //if (m_components.rBody.position.y > )

            //    bool correctionCast = Physics.SphereCast(
            //        correctionPoint,
            //        m_castRadius,
            //        Vector3.down,
            //        out RaycastHit correctionHit,
            //        distance,
            //        1 << m_layerNum
            //        );

            //if (correctionCast)
            //{

            //}
        }
        // float angle = Vector3.Angle(Vector3.up, hit.normal);

        return direction;
    }

    private void fallObject()
    {
        if (CState.isGrounded)
            Gravity = 0f;
        else
            Gravity += Constants.GRAVITY * m_fixedDeltaTime;
    }
}
