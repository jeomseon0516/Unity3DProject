using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const int X = 0;
    private const int Y = 1;
    private const float MAX_DISTANCE = 32.5f;
    private const float MIN_DISTANCE = 10.0f;

    [SerializeField, Range(0.0f, 360.0f)] private float[] _cameraAngle;
    [SerializeField, Range(MIN_DISTANCE, MAX_DISTANCE)] private float _distance;
    [SerializeField] private Vector3 _lookOffset;
    [SerializeField] private Vector3 _offset;
    private Vector3 _beforeMousePosition;
    private float _dragSpeed;

    [field:SerializeField] public GameObject TargetObject { get; set; }

    void Start()
    {
        _beforeMousePosition = Input.mousePosition;
        _cameraAngle = new float[] { 0.0f, 0.0f };
        _lookOffset = new Vector3(0.0f, 15.0f, 0.0f);
        _offset = new Vector3(0.0f, 18.5f, 0.0f);
        _distance = 20.0f;
        _dragSpeed = 15.0f;
    }
    void Update()
    {
        ControlMouseWheel();
        ControlMouseDrag();
        FocusingCamera();
    }
    void FocusingCamera()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        Vector3 cameraDirection = Quaternion.Euler(_cameraAngle[Y], _cameraAngle[X], 0.0f) * Vector3.back;
        Vector3 targetDirection = ((TargetObject.transform.position + _lookOffset) - transform.position).normalized;

        Vector3 targetPosition = TargetObject.transform.position + _offset + cameraDirection * _distance;

        float distance = Vector3.Distance(targetPosition, transform.position);
        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * distance * Time.deltaTime * 3f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * 10.0f);
    }
    void ControlMouseWheel()
    {
        float wheelInput = 2000.0f * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;

        _distance += wheelInput;

        if (_distance < MIN_DISTANCE)
            _distance = MIN_DISTANCE;
        else if (_distance > MAX_DISTANCE)
            _distance = MAX_DISTANCE;
    }
    void ControlMouseDrag()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 interval = Input.mousePosition - _beforeMousePosition;

            _cameraAngle[X] += interval.x * _dragSpeed * Time.deltaTime;
            _cameraAngle[Y] += interval.y * _dragSpeed * Time.deltaTime;
        }

        _beforeMousePosition = Input.mousePosition;
    }
}