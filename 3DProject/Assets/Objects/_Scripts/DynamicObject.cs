using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * .. 해당 컴포넌트는 중력이나 벽충돌에 관한 처리를 담당합니다. 
 * 유니티는 컴포넌트 패턴으로 구현되어 있다.
 * 컴포넌트 패턴은 하나의 클래스에 많은 기능이 집중되어 유지보수가 어려워지는걸 방지하기 위한 디자인 패턴이다.
 * Context 클래스의 기능이나 동작을 다시 클래스의 단위로 분리하여 동작을 수행하게끔 하는 패턴이다.
 * 상속 관계가 커지는 것도 마찬 가지이다. 그래서 유니티 또한 컴포넌트들이 GameObject를 상속받는 것이 아닌
 * GameObject가 Component List를 가지고 각 컴포넌트들의 동작을 수행하게끔 설계되어 있는 것이다. 
 */

[RequireComponent(typeof(WorldCollision))]
public abstract class DynamicObject : MonoBehaviour
{
    protected class MoveOption
    {
        public Vector3 direction;
        public Vector3 lookAt;
    }

    protected MoveOption m_moveOption = new MoveOption();

    protected WorldCollision m_worldCollision;

    protected float m_jumpingPower;
    [field: SerializeField, Range(0.0f, 8.0f)] public float Speed { get; set; }

    protected abstract void CustomAwake();
    protected abstract void CustomStart();
    protected abstract void CustomUpdate();
    protected abstract void CustomFixedUpdate();

    /*
     * .. 내리막길 Slope처리로 인해 점프시 지속적으로 땅에 붙는 현상 발생.
     * 로직의 순서 문제.
     * 점프 신호나 추락인지 판단 -> 점프나 공중일시 중력값을 받아옴 -> 이동 처리 -> 지형이냐 공중이냐에 따른 Slope 보정 -> 최종적인 Velocity 계산 -> 이동  
     */
    private void Awake()
    {
        m_moveOption.direction = Vector3.zero;
        m_moveOption.lookAt    = Vector3.zero;

        m_jumpingPower = 10.0f;
        
        TryGetComponent(out m_worldCollision);
        CustomAwake();
    }
    private void Start()
    {
        CustomStart();
    }
    private void Update()
    {
        CustomUpdate();
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_moveOption.lookAt), 20.0f * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        CustomFixedUpdate();
        m_worldCollision.MoveObject(m_moveOption.direction, Speed);
    }
    protected void SetJump()
    {
        m_worldCollision.Gravity = m_jumpingPower;
        m_worldCollision.IsJump = true;
    }
}
