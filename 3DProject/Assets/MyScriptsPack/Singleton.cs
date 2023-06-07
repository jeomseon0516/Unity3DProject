using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ISingleton
{
    void Init();
}

// .. 모노비하이비어를 상속받는 싱글톤 템플릿 입니다. ISingleton의 함수를 정의하여 사용해야 합니다.
public class Singleton<T> : MonoBehaviour where T : Singleton<T>, ISingleton
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

    private void Awake()
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
    protected Singleton() { }
}
