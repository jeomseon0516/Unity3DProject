using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * .. 상태를 객체화 한다. 
 */

// .. IState는 개별적으로 따로 동작할 수 없도록 internal 함수로 정의 한다.
public interface IState<T>
{
    internal void Awake(T t);
    internal void Enter(T t);
    internal void Update(T t);
    internal void Exit(T t);
}

public interface IStateObject<T>
{
    public void RegistStates();
}

public class StateMachine<T> where T : IStateObject<T>
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