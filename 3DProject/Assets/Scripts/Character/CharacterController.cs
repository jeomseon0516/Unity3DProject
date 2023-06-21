using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : DynamicObject
{
    Camera _mainCamera;
    protected override void CustomAwake()
    {
        GameObject.Find("Main Camera").TryGetComponent(out _mainCamera);
    }
    protected override void Init()
    {
        _speed = 10.0f;
    }
    protected override void CustomUpdate()
    {
        Direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        Vector3 lookAt = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

        LookAt = new Vector3(Mathf.Abs(lookAt.x) > float.Epsilon ? _mainCamera.transform.forward.x + lookAt.x : LookAt.x, 0.0f, 
                             Mathf.Abs(lookAt.z) > float.Epsilon ? _mainCamera.transform.forward.z + lookAt.z : LookAt.z);
    }
}
