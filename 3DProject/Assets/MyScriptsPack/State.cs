using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * .. 상태를 객체화 한다. 
 */

public interface IState<T> where T : DynamicObject
{
    public void Awake(T t);
    public void Enter(T t);
    public void Update(T t);
    public void Exit(T t);
}
// .. Context객체에서 직접 IState의 함수들을 호출 할 권한을 주지 않습니다.
public class StateMachine<T> where T : DynamicObject
{
    private Dictionary<string, IState<T>> _stateList = new(); // .. 상태들을 저장 상태들을 매번 new로 생성하면 가비지가 생성되고 비용이 크기 때문
    private IState<T> _state; // .. 현재 상태
    public void Update(T t)
    {
        _state.Update(t);
    }
    public void RegistState(T t, string key, IState<T> state)
    {
        if (_stateList.ContainsKey(key)) return;

        state.Awake(t);
        _stateList.Add(key, state);
        _state = state;
    }
    public void ChangeState(T t, string key)
    {
        if (!_stateList.TryGetValue(key, out IState<T> state) || _state == state) return;

        _state.Exit(t);
        _state = state;
        _state.Enter(t);
    }
}