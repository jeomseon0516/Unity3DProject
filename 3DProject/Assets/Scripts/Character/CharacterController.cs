using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : DynamicObject
{
    Camera _mainCamera;
    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.red);
    }
    protected override void CustomAwake()
    {
        GameObject.Find("Main Camera").TryGetComponent(out _mainCamera);
    }
    protected override void Init()
    {
        _speed = 20.0f;
    }
    protected override void CustomUpdate()
    {
        Direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        LookAt = Quaternion.LookRotation(new Vector3(_mainCamera.transform.forward.x, 0.0f, _mainCamera.transform.forward.z)) * 
                                         new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
    }
}
