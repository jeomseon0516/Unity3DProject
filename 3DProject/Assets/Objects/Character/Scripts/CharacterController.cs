using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DynamicObject))]
public partial class CharacterController : MonoBehaviour, IDynamicObject
{
    public class Components
    {
        public DynamicObject dynamicObject; // .. 상속 관계에 있던 다이나믹 오브젝트를 컴포넌트로 구조 변경
        public Camera mainCamera;
        public Animator animator;
        public Rigidbody rBody;
    }
    private const float DEFAULT_SPEED = 6.0f;
    private Components m_components = new Components();
    public Components Com => m_components;

    private StateMachine<CharacterController> m_stateMachine;
    private float m_runSpeed;
    private float m_jumpValue;

    private void Awake()
    {
        initDynamicObject();
        initComponent();
    }
    private void Start()
    {
        m_stateMachine = new StateMachine<CharacterController>();

        m_stateMachine.RegistState(this, "Fall",    new FallState());
        m_stateMachine.RegistState(this, "Default", new DefaultState());
        m_stateMachine.ChangeState(this, "Default");

        m_runSpeed = DEFAULT_SPEED * 1.5f;
    }
    private void Update()
    {
        m_stateMachine.Update(this);
    }
#if UNITY_EDITOR_WIN
    private void FixedUpdate()
    {
        Debug.DrawRay(m_components.rBody.position, transform.forward * 5.0f, Color.red);
    }
#endif
    private void initComponent()
    {
        GameObject.Find("Main Camera").TryGetComponent(out m_components.mainCamera);
        TryGetComponent(out m_components.animator);
        TryGetComponent(out m_components.rBody);
    }
    private void initDynamicObject()
    {
        if (!TryGetComponent(out m_components.dynamicObject)) return;

        m_components.dynamicObject.Speed = DEFAULT_SPEED;
    }
}

public partial class CharacterController : MonoBehaviour, IDynamicObject
{
    // .. 행동을 상태에다 위임 ..
    private sealed class DefaultState : IState<CharacterController>
    {
        DynamicObject m_dynamicObject;
        Animator m_animator;
        Camera m_mainCamera;

        public void Awake(CharacterController character)
        {
            m_animator      = character.m_components.animator;
            m_mainCamera    = character.m_components.mainCamera;
            m_dynamicObject = character.m_components.dynamicObject;
        }
        public void Enter(CharacterController character)
        {

        }
        public void Update(CharacterController character)
        {
            m_dynamicObject.Direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            m_animator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(m_dynamicObject.Direction.x), Mathf.Abs(m_dynamicObject.Direction.z)));

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
                Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift) && !ReferenceEquals(m_animator, null) && m_animator)
                {
                    m_animator.SetBool("isSprint", true);
                    m_animator.speed = 1.2f;
                }

                m_dynamicObject.LookAt = Quaternion.LookRotation(new Vector3(
                    m_mainCamera.transform.forward.x,
                    0.0f,
                    m_mainCamera.transform.forward.z)) *
                    new Vector3(m_dynamicObject.Direction.x, 0.0f, m_dynamicObject.Direction.z);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) && !ReferenceEquals(m_animator, null) && m_animator)
            {
                m_animator.SetBool("isSprint", false);
                m_animator.speed = 1;
            }

            m_dynamicObject.Speed = 
                Input.GetKey(KeyCode.LeftShift) ? 
                character.m_runSpeed : 
                CharacterController.DEFAULT_SPEED;

            // .. 여기서 상태 변경
            if (!m_dynamicObject.IsGrounded)
                character.m_stateMachine.ChangeState(character, "Fall");
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    character.m_stateMachine.ChangeState(character, "Fall");
                    m_dynamicObject.Gravity = 20.0f;
                }
            }
        }
        public void Exit(CharacterController character)
        {
            m_animator.SetFloat("moveSpeed", 0.0f);
            m_animator.SetBool("isSprint",   false);
            m_animator.speed = 1;
        }
    }
    private sealed class FallState : IState<CharacterController>
    {
        DynamicObject m_dynamicObject;
        public void Awake(CharacterController character)
        {
            m_dynamicObject = character.m_components.dynamicObject;
        }
        public void Enter(CharacterController character)
        { 
        }
        public void Update(CharacterController character)
        {
            if (m_dynamicObject.GroundInterval <= 0)
            {
                character.m_stateMachine.ChangeState(character, "Default");
                character.m_components.rBody.MovePosition(
                    character.m_components.rBody.position - 
                    new Vector3(0.0f, m_dynamicObject.GroundInterval, 0.0f));
            }
        }
        public void Exit(CharacterController character)
        {
        }
    }
}