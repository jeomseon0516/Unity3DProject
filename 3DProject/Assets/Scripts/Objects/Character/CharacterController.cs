using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterController : DynamicObject
{
    private Camera  _mainCamera;
    private Animator _animator;

    private float _defaultSpeed;
    private float _runSpeed;
    private float _jumpValue;

    private WorldCollision _worldCollision;

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.red);
    }
    protected override void CustomAwake()
    {
        GameObject.Find("Main Camera").TryGetComponent(out _mainCamera);

        TryGetComponent(out _worldCollision);
        TryGetComponent(out _animator);
    }
    protected override void Init()
    {
        _defaultSpeed = _speed = 6.0f;
        _runSpeed = _speed * 1.5f;
        _jumpValue = 0.0f;
    }
    protected override void CustomUpdate()
    {
        Direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        // 애니메이션 결정
        decideAnimationState();

        if (!_worldCollision.OnPlaneCollision)
        {
            
        }
        else
        {
            _jumpValue = 0.0f;
        }

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || 
            Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !ReferenceEquals(_animator, null) && _animator)
            {
                _animator.SetBool("isSprint", true);
                _animator.speed = 1.2f;
            }

            LookAt = Quaternion.LookRotation(new Vector3(
                _mainCamera.transform.forward.x, 
                0.0f, 
                _mainCamera.transform.forward.z)) * 
                new Vector3(Direction.x, 0.0f, Direction.z);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) && !ReferenceEquals(_animator, null) && _animator)
        {
            _animator.SetBool("isSprint", false);
            _animator.speed = 1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {

        }

        _speed = !Input.GetKey(KeyCode.LeftShift) ? _defaultSpeed : _runSpeed; 
    }
}
// 애니메이션
public partial class CharacterController : DynamicObject
{
    private void decideAnimationState()
    {
        if (ReferenceEquals(_animator, null) || !_animator) return;

        _animator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(Direction.x), Mathf.Abs(Direction.z)));
    }
}