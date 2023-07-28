using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// .. 상태 패턴을 사용하기 때문에 private으로 선언된 필드들을 모두 프로퍼티로 변경 .. 
public partial class CharacterController : DynamicObject
{
    private const float DEFAULT_SPEED = 6.0f;
    private StateMachine<CharacterController> m_stateMachine;
    private Camera m_mainCamera;
    private Animator m_animator;
    private float m_runSpeed;
    private float m_jumpValue;

#if UNITY_EDITOR_WIN
    protected override void CustomFixedUpdate()
    {
        Debug.DrawRay(m_rigidbody.position, transform.forward * 5.0f, Color.red);
    }
#endif
    protected override void CustomAwake()
    {
        m_stateMachine = new StateMachine<CharacterController>();
        m_mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        TryGetComponent(out m_animator);
    }
    protected override void Init()
    {
        m_stateMachine.RegistState(this, "Fall",    new FallState());
        m_stateMachine.RegistState(this, "Default", new DefaultState());
        m_stateMachine.ChangeState(this, "Default");

        m_speed    = DEFAULT_SPEED;
        m_runSpeed = DEFAULT_SPEED * 1.5f;
    }
    protected override void CustomUpdate()
    {
        m_stateMachine.Update(this);
    }
}
public partial class CharacterController : DynamicObject
{
    // .. 행동을 상태에다 위임 ..
    private sealed class DefaultState : IState<CharacterController>
    {
        public void Awake(CharacterController character)
        {
        }
        public void Enter(CharacterController character)
        {
        }
        public void Update(CharacterController character)
        {
            character.Direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

            character.m_animator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(character.Direction.x), Mathf.Abs(character.Direction.z)));

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
                Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift) && !ReferenceEquals(character.m_animator, null) && character.m_animator)
                {
                    character.m_animator.SetBool("isSprint", true);
                    character.m_animator.speed = 1.2f;
                }

                character.LookAt = Quaternion.LookRotation(new Vector3(
                    character.m_mainCamera.transform.forward.x,
                    0.0f,
                    character.m_mainCamera.transform.forward.z)) *
                    new Vector3(character.Direction.x, 0.0f, character.Direction.z);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) && !ReferenceEquals(character.m_animator, null) && character.m_animator)
            {
                character.m_animator.SetBool("isSprint", false);
                character.m_animator.speed = 1;
            }

            character.m_speed = !Input.GetKey(KeyCode.LeftShift) ? CharacterController.DEFAULT_SPEED : character.m_runSpeed;

                // .. 여기서 상태 변경
            if (character.m_worldCollision.IsFall)
                character.m_stateMachine.ChangeState(character, "Fall");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                character.m_stateMachine.ChangeState(character, "Fall");
                character.m_worldCollision.Gravity = 20.0f;
            }
        }
        public void Exit(CharacterController character)
        {
            character.m_animator.SetFloat("moveSpeed", 0.0f);
            character.m_animator.SetBool("isSprint",   false);
            character.m_animator.speed = 1;
        }
    }
    private sealed class FallState : IState<CharacterController>
    {
        public void Awake(CharacterController character)
        {
        }
        public void Enter(CharacterController character)
        { 
        }
        public void Update(CharacterController character)
        {
            if (!character.m_worldCollision.IsFall)
                character.m_stateMachine.ChangeState(character, "Default");
        }
        public void Exit(CharacterController character)
        {
        }
    }
}