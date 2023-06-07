using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SingletonBase;

public interface ISingleton 
{
    void Init();
}

namespace SingletonBase
{
    public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
    {
        protected abstract void Awake();
        protected SingletonBase() {}
    }
}

// .. 모노비하이비어를 상속받는 싱글톤 템플릿 입니다. ISingleton의 함수를 정의하여 사용해야 합니다.
//    Awake 오버라이딩시 정상적으로 동작하지 않습니다.
//    Awake 대신 Init메서드를 오버라이딩 후 사용해주세요.

public abstract class Singleton<T> : SingletonBase<Singleton<T>> where T : Singleton<T>, ISingleton
{
    private static T instance = null;

    public static T GetInstance
    {
        get
        {
            CreateInstance(() => { instance = new GameObject().AddComponent<T>(); });
            return instance;
        }
    }

    protected override sealed void Awake()
    {
        if (CreateInstance(() => { TryGetComponent(out instance); }))
            Destroy(gameObject);
    }

    private static bool CreateInstance(Action action)
    {
        if (instance is null)
        {
            if ((instance = FindObjectOfType<T>()) is null)
                action();
            else
                return false;

            instance.name = typeof(T).ToString();
            instance.Init();
        }

        return true;
    }

    protected Singleton() {}
}