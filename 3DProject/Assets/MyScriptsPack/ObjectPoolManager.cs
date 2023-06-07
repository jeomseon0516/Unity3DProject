using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>, ISingleton
{
    public void Init()
    {
        print("HelloWorld!");
    }

    void Update()
    {

    }

    public void Hello() {}

    private ObjectPoolManager() {}
}
