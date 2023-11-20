using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * .. 상태를 객체화 한다. 
 */

// .. IState는 개별적으로 따로 동작할 수 없도록 internal 함수로 정의 한다.
public interface IState<T>
{
    internal void Awake(T t); // .. 초기에 상태가 생성될 시에만 해당 함수 호출 ..
    internal void Enter(T t); // .. 상태가 변경됐을때 Update 전에 한번만 호출 ..
    internal void Update(T t); // .. 상태가 변경되기 전까지 계속해서 호출 ..
    internal void Exit(T t); // .. 상태가 변경됐을때 호출 ..
}

public interface IStateObject
{
    public void RegistStates();
}

public class StateMachine<T> where T : IStateObject // .. State의 관리는 StateMachine 만 수행하도록 구조가 짜여져있다.
{
    private Dictionary<string, IState<T>> _stateList = new Dictionary<string, IState<T>>(); // .. 상태들을 저장. 상태들을 매번 new로 생성하면 가비지가 생성되고 비용이 크기 때문
    private IState<T> _state; // .. 현재 상태
    public void Update(T t)
    {
        _state.Update(t);
    }
    public void RegistState(T t, string key, IState<T> state) // .. 상태를 저장
    {
        if (_stateList.ContainsKey(key) || _stateList.ContainsValue(state)) return;

        state.Awake(t);
        _stateList.Add(key, state);
        _state = state;
    }
    public void ChangeState(T t, string key) // .. 딕셔너리에서 저장 된 상태를 가져옴 
    {
        if (!_stateList.TryGetValue(key, out IState<T> state) || _state == state) return;

        _state.Exit(t);
        _state = state;
        _state.Enter(t);
    }
}