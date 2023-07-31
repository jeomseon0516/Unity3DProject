using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDynamicObject
{
    public void initDynamicObject() {}
}

[RequireComponent(typeof(WorldCollision))]
public class DynamicObject : MonoBehaviour
{
    public class Components
    {
        public WorldCollision worldCollision;
        public Rigidbody rBody;
    }
    public class MoveOption
    {
        public Vector3 direction;
        public Vector3 lookAt;
    }

    private float _fixedDeltaTime = Time.fixedDeltaTime;
    private Components m_components = new Components();
    private MoveOption m_moveOption = new MoveOption();

    [field:SerializeField, Range(0.0f, 8.0f)] public float Speed { get; set; }
    public Components Com => m_components;
    public Vector3 Direction 
    { 
        get => m_moveOption.direction; 
        set => m_moveOption.direction = value; 
    }
    public Vector3 LookAt
    {
        get => m_moveOption.lookAt;
        set => m_moveOption.direction = value;
    }

    private void Awake()
    {

    }
    private void Start()
    {
        Speed = 10.0f;
        m_moveOption.direction = new Vector3(0.0f, 0.0f, 0.0f);
        m_moveOption.lookAt = m_moveOption.direction;
    }
    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_moveOption.lookAt), 20.0f * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        float max = Mathf.Max(Mathf.Abs(m_moveOption.direction.x), Mathf.Abs(m_moveOption.direction.z));
        
        Vector3 movement = (transform.forward * Speed * 50 * max * _fixedDeltaTime) +
            new Vector3(0.0f, m_components.worldCollision.Gravity, 0.0f);

        m_components.rBody.velocity = movement;
    }
    private void initComponents()
    {
        TryGetComponent(out m_components.rBody);
        TryGetComponent(out m_components.worldCollision);
    }
}
