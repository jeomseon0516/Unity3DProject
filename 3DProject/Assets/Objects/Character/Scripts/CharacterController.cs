using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(DynamicObject))]
public partial class CharacterController : MonoBehaviour, IStateObject
{
    public class Components
    {
        public Camera mainCamera;
        public Animator animator;
        public DynamicObject dynamicObject;
    }
    public class MoveOption
    {
        public Vector3 lookAt;
        public Vector3 direction;
        public float speed;
        public float runSpeed;
        public float jumpingPower;
    }

    private const float DEFAULT_SPEED = 6.0f;
    private Components m_components = new Components();
    private MoveOption m_moveOption = new MoveOption();
    private StateMachine<CharacterController> m_stateMachine = new StateMachine<CharacterController>();
    public Components Com => m_components;
    public MoveOption MOption => m_moveOption;

    private void Awake()
    {
        initComponents();
    }
    private void Start()
    {
        RegistStates();
        initMoveOption();
    }
    private void Update()
    {
        m_stateMachine.Update(this);
    }
    public void RegistStates()
    {
        m_stateMachine.RegistState(this, "Fall",    new FallState());
        m_stateMachine.RegistState(this, "Default", new DefaultState());
        m_stateMachine.ChangeState(this, "Default");
    }
    private void initMoveOption()
    {
        m_moveOption.speed = DEFAULT_SPEED;
        m_moveOption.runSpeed = DEFAULT_SPEED * 1.5f;
        m_moveOption.jumpingPower = 5.0f;
        m_moveOption.lookAt = Vector3.zero;
        m_moveOption.direction = Vector3.zero;
    }
    private void initComponents()
    {
        GameObject.Find("Main Camera").TryGetComponent(out m_components.mainCamera);
        TryGetComponent(out m_components.animator);
        TryGetComponent(out m_components.dynamicObject);
    }
}

public partial class CharacterController : MonoBehaviour
{
    // .. 행동을 상태에다 위임 ..
    private sealed class DefaultState : IState<CharacterController>
    {
        Camera m_mainCamera;
        Animator m_animator;
        MoveOption m_moveOption;
        DynamicObject m_dynamicObject;
        void IState<CharacterController>.Awake(CharacterController character)
        {
            m_moveOption    = character.m_moveOption;
            m_animator      = character.m_components.animator;
            m_mainCamera    = character.m_components.mainCamera;
            m_dynamicObject = character.m_components.dynamicObject;
        }
        void IState<CharacterController>.Enter(CharacterController character)
        {

        }
        void IState<CharacterController>.Update(CharacterController character)
        {
            m_moveOption.direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            m_animator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(m_moveOption.direction.x), Mathf.Abs(m_moveOption.direction.z)));

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
                Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift) && !ReferenceEquals(m_animator, null) && m_animator)
                {
                    m_animator.SetBool("isSprint", true);
                    m_animator.speed = 1.2f;
                }

                m_moveOption.lookAt = Quaternion.LookRotation(
                    new Vector3(
                    m_mainCamera.transform.forward.x,
                    0.0f,
                    m_mainCamera.transform.forward.z)
                    ) * m_moveOption.direction;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                m_animator.SetBool("isSprint", false);
                m_animator.speed = 1;
            }

            m_moveOption.speed = 
                Input.GetKey(KeyCode.LeftShift) ? 
                m_moveOption.runSpeed : 
                CharacterController.DEFAULT_SPEED;

            if (Input.GetKeyDown(KeyCode.Space))
                m_dynamicObject.SetJump();

            m_dynamicObject.MoveObject(m_moveOption.direction, m_moveOption.lookAt, m_moveOption.speed);

            // .. 여기서 상태 변경
            if (!m_dynamicObject.CState.isGrounded)
                character.m_stateMachine.ChangeState(character, "Fall");
        }
        void IState<CharacterController>.Exit(CharacterController character)
        {
            m_animator.SetFloat("moveSpeed", 0.0f);
            m_animator.SetBool("isSprint", false);
            m_animator.speed = 1;
        }
    }
    private sealed class FallState : IState<CharacterController>
    {
        DynamicObject m_dynamicObject;
        void IState<CharacterController>.Awake(CharacterController character)
        {
            m_dynamicObject = character.m_components.dynamicObject;
        }
        void IState<CharacterController>.Enter(CharacterController character)
        { 
        }
        void IState<CharacterController>.Update(CharacterController character)
        {
            if (m_dynamicObject.CState.isGrounded)
                character.m_stateMachine.ChangeState(character, "Default");
        }
        void IState<CharacterController>.Exit(CharacterController character)
        {
        }
    }
}