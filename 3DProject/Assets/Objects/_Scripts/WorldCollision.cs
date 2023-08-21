using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MyGizmo))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class WorldCollision : MonoBehaviour
{
    #region MField
    /*
     * -----------------------------------------------------------------------------------------
     *                               MemberField
     * -----------------------------------------------------------------------------------------
     */

    private readonly float CHECK_GROUND_DISTANCE = 0.01f; // .. 지면인식 허용 거리 내리막길에서 튀는 현상 방지
    private readonly float CHECK_MAX_GROUND_INTERVAL = 0.0001f; // .. 지면 체크를 할때 내보정값.
    private readonly float SPHERE_MAX_DISTANCE = 2.0f; // .. Sphere 캐스팅할 거리
    private readonly float HORIZONTAL_SLOPE_ANGLE_RATIO = 1 / 90f; // .. 90도를 1의 비율로 나타내었을때의 값)
    public class Components
    {
        public MyGizmo sphereGizmo;
        public MyGizmo groundCollisionGizmo;
        public CapsuleCollider capsuleCollider;
        public Rigidbody rigidbody;
    }
    public class MoveOption
    {
        public Vector3 horizontalVelocity;
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
    #endregion
    private void Awake()
    {
        initRigidbody();
        initCapsuleCollider();

        m_castRadius = m_components.capsuleCollider.radius * 0.9f;
        m_castRadiusDiff = m_components.capsuleCollider.radius - m_castRadius;

#if UNITY_EDITOR_WIN
        initGizmos();
#endif

        Gravity = 0f;

        m_collisionState.isGrounded = false;
        m_collisionState.isForwardBlocked = false;

        m_layerNum = LayerMask.NameToLayer("Plane");
        IsJump = false;
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
    public void MoveObject(Vector3 direction, float speed)
    {
        m_fixedDeltaTime = Time.fixedDeltaTime;

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

        m_components.sphereGizmo.Pivot = m_components.rigidbody.position + new Vector3(0.0f, m_castRadius, 0.0f);
        m_components.rigidbody.velocity = m_moveOption.horizontalVelocity + Vector3.up * Gravity;
    }

    // .. 해당 함수는 지면을 체크합니다. 지면의 경사각을 체크하여 투영된 방향을 구해줍니다.
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

            m_collisionState.isGrounded = hit.point.y + CHECK_MAX_GROUND_INTERVAL >= transform.position.y ;

            // .. angle값 벡터를 내적하여 각도로 변환한 것과 같다.
            /*
             *  .. 아래와 같음
             *  float dot = Vector3.Dot(hit.normal, Vector3.up); 코사인 값
             *  float radian = Mathf.Acos(dot);
             *  float angle = radian * Mathf.Rad2Deg;
             */

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
}
