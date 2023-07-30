using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldCollision))]
public abstract class DynamicObject : MonoBehaviour
{
    [SerializeField, Range(0.0f, 8.0f)] protected float m_speed;

    protected WorldCollision m_worldCollision;
    protected Rigidbody m_rigidbody;
    public Vector3 Direction { get; protected set; } 
    public Vector3 LookAt { get; protected set; }

    private void Awake()
    {
        CustomAwake();
        TryGetComponent(out m_rigidbody);
        TryGetComponent(out m_worldCollision);
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
    }
    private void FixedUpdate()
    {
        CustomFixedUpdate();

        float max = Mathf.Max(Mathf.Abs(Direction.x), Mathf.Abs(Direction.z));
        m_rigidbody.velocity = (transform.forward * m_speed * 50 * max * Time.fixedDeltaTime) + new Vector3(0.0f, m_worldCollision.Gravity, 0.0f);
    }
    protected abstract void Init();
    protected abstract void CustomAwake();
    protected abstract void CustomUpdate();
    protected abstract void CustomFixedUpdate();
}
