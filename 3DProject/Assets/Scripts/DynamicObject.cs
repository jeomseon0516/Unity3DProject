using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicObject : MonoBehaviour
{
    [field:SerializeField, Range(0.0f, 8.0f)] protected float m_speed { get; set; }

    public Vector3 Direction { get; protected set; } 
    public Vector3 LookAt { get; protected set; }

    private void Awake()
    {
        CustomAwake();
    }
    private void Start()
    {
        m_speed = 10.0f;
        Direction = new Vector3(0.0f, 0.0f, 0.0f);
        LookAt = Direction;

        Init();
    }
    private void Update()
    {
        CustomUpdate();

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(LookAt), 20.0f * Time.deltaTime);

        float max = Mathf.Max(Mathf.Abs(Direction.x), Mathf.Abs(Direction.z));
        transform.position += transform.forward * m_speed * max * Time.deltaTime;
    }
    protected abstract void Init();
    protected abstract void CustomAwake();
    protected abstract void CustomUpdate();
}
