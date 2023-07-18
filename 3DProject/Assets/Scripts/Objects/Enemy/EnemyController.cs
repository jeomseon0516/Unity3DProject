using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AStar))]
public partial class EnemyController : MonoBehaviour
{
    private bool _isMove;
    private float _speed;

    private Vector3 _targetPoint;
    private Rigidbody _rigidbody;
    private AStar _aStar;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _aStar = GetComponent<AStar>();
    }
    void Start()
    {
        _isMove = true;

        _rigidbody.useGravity = false;
        _targetPoint = Vector3.zero;

        _speed = 5.0f;

        _aStar.Size = 0.6f;
        _aStar.TargetObject = GameObject.Find("Character");
        _aStar.FindPath();
        _targetPoint = _aStar.GetMoveNext();
    }
    void Update()
    {
        move();
    }
    private void move()
    {
        if (Vector3.Distance(_targetPoint, transform.position) < 0.1f)
        {
            if (!_aStar.ContainWays())
                _aStar.FindPath();

            _targetPoint = _aStar.GetMoveNext();
        }

        Vector3 direction = (_targetPoint - transform.position).normalized;
        transform.position += direction * _speed * Time.deltaTime;
    }
    private IEnumerator setRotation()
    {
        float time;
        _isMove = true;

        int count = UnityEngine.Random.Range(2, 4) + 1;

        // 고개 돌리기
        //for (int i = 0; i < count; ++i)
        //{
        //    time = 0.0f;

        //    float radian = CustomMath.ConvertFromAngleToRadian(transform.eulerAngles.y + Random.Range(-Angle, Angle));

        //    while (time < 1.0f)
        //    {
        //        transform.rotation = Quaternion.Lerp(
        //            transform.rotation,
        //            Quaternion.LookRotation(new Vector3(Mathf.Sin(radian), 0.0f, Mathf.Cos(radian))),
        //            0.016f);

        //        time += Time.deltaTime;
        //        yield return null;
        //    }
        //}

        time = 0.0f;

        while (time < 1.0f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation((_targetPoint - transform.position).normalized),
                Time.deltaTime * 10.0f);

            time += Time.deltaTime;

            yield return null;
        }

        _isMove = true;
    }
}