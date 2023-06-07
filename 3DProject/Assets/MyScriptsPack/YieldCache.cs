using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * .. 코루틴을 호출할때마다 매번 new 키워드를 사용시 생기는 가비지를 방지하기 위해 
 */
public static class YieldCache
{
    private static readonly Dictionary<float, WaitForSeconds> time = new Dictionary<float, WaitForSeconds>();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

    public static WaitForSeconds WaitForSeconds(float _time)
    {
        if (!time.TryGetValue(_time, out WaitForSeconds wfs))
            time.Add(_time, wfs = new WaitForSeconds(_time));

        return wfs;
    }
}
