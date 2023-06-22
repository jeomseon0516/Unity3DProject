using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const int X = 0;
    private const int Y = 1;

    [SerializeField, Range(0.0f, 360.0f)] private float[] _cameraAngle;
    [SerializeField, Range(10, 30)] private float _maxDistance;
    [SerializeField] private Vector3 _lookOffset;
    [SerializeField] private Vector3 _offset;
    [field:SerializeField] public GameObject TargetObject { get; set; }

    void Start()
    {
        _cameraAngle = new float[] { 0.0f, 0.0f };
        _lookOffset = new Vector3(0.0f, 15.0f, 0.0f);
        _offset = new Vector3(0.0f, 23.0f, 0.0f);
        _maxDistance = 20.0f;
    }

    void Update()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        Vector3 cameraDirection = Quaternion.Euler(_cameraAngle[Y], _cameraAngle[X], 0.0f) * Vector3.forward;
        Vector3 targetDirection = ((TargetObject.transform.position + _lookOffset) - transform.position).normalized;

        Vector3 targetPosition = TargetObject.transform.position + _offset + cameraDirection * _maxDistance;

        float distance = Vector3.Distance(targetPosition, transform.position);
        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * distance * Time.deltaTime * 3f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * 10.0f);
    }
}
