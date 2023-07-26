using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// .. 상태 패턴을 사용하기 때문에 private으로 선언된 필드들을 모두 프로퍼티로 변경 ..
[RequireComponent(typeof(WorldCollision))]
public partial class CharacterController : DynamicObject
{
    private const float DEFAULT_SPEED = 6.0f;
    private StateMachine<CharacterController> StateMachine { get; } = new StateMachine<CharacterController>();
    private WorldCollision MainWorldCollision { get; set; }
    private Camera MainCamera { get; set; }
    private Animator MainAnimator { get; set; }
    private float RunSpeed { get; set; }
    private float JumpValue { get; set; }

#if UNITY_EDITOR_WIN
    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.red);
    }
#endif
    protected override void CustomAwake()
    {
        MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        MainWorldCollision = GetComponent<WorldCollision>();
        MainAnimator = GetComponent<Animator>();
    }
    protected override void Init()
    {
        StateMachine.RegistState(this, "Fall",    new FallState());
        StateMachine.RegistState(this, "Default", new DefaultState());
        StateMachine.ChangeState(this, "Default");

        Speed = DEFAULT_SPEED;
        RunSpeed = DEFAULT_SPEED * 1.5f;
    }
    protected override void CustomUpdate()
    {
        StateMachine.Update(this);
    }
}
public partial class CharacterController : DynamicObject
{
    // .. 행동을 상태에다 위임 ..
    public sealed class DefaultState : IState<CharacterController>
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

            character.MainAnimator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(character.Direction.x), Mathf.Abs(character.Direction.z)));

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift) && !ReferenceEquals(character.MainAnimator, null) && character.MainAnimator)
                {
                    character.MainAnimator.SetBool("isSprint", true);
                    character.MainAnimator.speed = 1.2f;
                }

                character.LookAt = Quaternion.LookRotation(new Vector3(
                    character.MainCamera.transform.forward.x,
                    0.0f,
                    character.MainCamera.transform.forward.z)) *
                    new Vector3(character.Direction.x, 0.0f, character.Direction.z);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) && !ReferenceEquals(character.MainAnimator, null) && character.MainAnimator)
            {
                character.MainAnimator.SetBool("isSprint", false);
                character.MainAnimator.speed = 1;
            }

            character.Speed = !Input.GetKey(KeyCode.LeftShift) ? CharacterController.DEFAULT_SPEED : character.RunSpeed;

                // .. 여기서 상태 변경
            if (!character.MainWorldCollision.OnPlaneCollision)
                character.StateMachine.ChangeState(character, "Fall");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                character.StateMachine.ChangeState(character, "Fall");
                character.JumpValue = 20.0f;
            }
        }
        public void Exit(CharacterController character)
        {
            character.MainAnimator.SetFloat("moveSpeed", 0.0f);
            character.MainAnimator.SetBool("isSprint", false);
            character.MainAnimator.speed = 1;
        }
    }
    public sealed class FallState : IState<CharacterController>
    {
        public void Awake(CharacterController character)
        {
        }
        public void Enter(CharacterController character)
        {
            character.transform.position += new Vector3(0.0f, character.JumpValue * Time.deltaTime, 0.0f);
            character.JumpValue -= Constants.GRAVITY * Time.deltaTime;
        }
        public void Update(CharacterController character)
        {
            if (character.MainWorldCollision.OnPlaneCollision)
                character.StateMachine.ChangeState(character, "Default");

            character.transform.position += new Vector3(0.0f, character.JumpValue * Time.deltaTime, 0.0f);
            character.JumpValue -= Constants.GRAVITY * Time.deltaTime;
        }
        public void Exit(CharacterController character)
        {
            character.JumpValue = 0.0f;
        }
    }
}