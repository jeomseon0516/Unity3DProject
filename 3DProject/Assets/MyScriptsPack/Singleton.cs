using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SingletonBase;

public interface ISingleton 
{
    public void Init();
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

    public static T Instance
    {
        get
        {
            createInstance(() => { instance = new GameObject().AddComponent<T>(); });
            return instance;
        }
    }

    protected override sealed void Awake()
    {
        if (createInstance(() => { TryGetComponent(out instance); }))
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private static bool createInstance(Action action)
    {
        if (ReferenceEquals(instance, null) || !instance)
        {
            if (ReferenceEquals((instance = FindObjectOfType<T>()), null) || !instance)
                action();
            else
            {
                instance.name = typeof(T).ToString();
                instance.Init();

                return false;
            }
        }

        return true;
    }
    protected Singleton() {}
}