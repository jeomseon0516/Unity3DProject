using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float ANGLE_SPEED_MAX = 80.0f;
    private const float MAX_DISTANCE = 3.8f;
    private const float MIN_DISTANCE = 1.5f;
    private const float CAMERA_SPEED = 12.5f;
    private const float ROTATE_SPEED = 50.0f;
    private const float Y_ANGLE_MAX = 64.0f;
    private const float WHEEL_SPEED = 800.0f;
    private const float DRAG_SPEED = 15.0f;
    private const int X = 0;
    private const int Y = 1;

    [SerializeField, Range(0.0f, 360.0f)] private float[] _cameraAngle;
    [SerializeField, Range(MIN_DISTANCE, MAX_DISTANCE)] private float _distance;
    [SerializeField] private Vector3 _lookOffset;
    [SerializeField] private Vector3 _offset;

    private Vector3 _beforeMousePosition;

    [field:SerializeField] public GameObject TargetObject { get; set; }

    void Start()
    {
        _beforeMousePosition = Input.mousePosition;
        _cameraAngle = new float[] { 0.0f, 0.0f };
        _lookOffset = new Vector3(0.0f, 1.5f, 0.0f);
        _offset = new Vector3(0.0f, 1.0f, 0.0f);
        _distance = 5.0f;
    }
    void Update()
    {
        controlMouseWheel();
        controlMouseDrag();
        focusingCamera();
    }
    void focusingCamera()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        _cameraAngle[Y] = _cameraAngle[Y] % 360.0f;

        if      (_cameraAngle[Y] < -Y_ANGLE_MAX) // 특정 각도 이상으로 넘어가면 카메라가 휙휙 돌아가서 눈 아프므로 각도 제한을 준다.
            _cameraAngle[Y] = -Y_ANGLE_MAX;
        else if (_cameraAngle[Y] >  Y_ANGLE_MAX)
            _cameraAngle[Y] = Y_ANGLE_MAX;

        Vector3 cameraDirection = Quaternion.Euler(_cameraAngle[Y], _cameraAngle[X], 0.0f) * Vector3.back;
        Vector3 targetDirection = ((TargetObject.transform.position + _lookOffset) - transform.position).normalized;

        Vector3 targetPosition = TargetObject.transform.position + _offset + cameraDirection * _distance;

        float   distance  = Vector3.Distance(targetPosition, transform.position);
        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * distance * Time.deltaTime * CAMERA_SPEED;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * ROTATE_SPEED);
    }
    void controlMouseWheel()
    {
        float wheelInput = WHEEL_SPEED * -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;

        _distance += wheelInput;

        if      (_distance < MIN_DISTANCE)
            _distance = MIN_DISTANCE;
        else if (_distance > MAX_DISTANCE)
            _distance = MAX_DISTANCE;
    }
    private void controlMouseDrag()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // 갑자기 화면이 크게 돌아가는 현상을 방지
            _beforeMousePosition = Input.mousePosition;

        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 interval = Input.mousePosition - _beforeMousePosition;

            if (Mathf.Abs(interval.x) > ANGLE_SPEED_MAX)
                interval.x = interval.x < 0 ? -ANGLE_SPEED_MAX : ANGLE_SPEED_MAX;
            if (Mathf.Abs(interval.y) > ANGLE_SPEED_MAX)
                interval.y = interval.y < 0 ? -ANGLE_SPEED_MAX : ANGLE_SPEED_MAX;

            _cameraAngle[X] +=  interval.x * DRAG_SPEED * Time.deltaTime;
            _cameraAngle[Y] += -interval.y * DRAG_SPEED * Time.deltaTime;

            _beforeMousePosition = Input.mousePosition;
        }
    }
}