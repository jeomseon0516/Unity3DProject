using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/* .. 해당 클래스는 유니티의 JsonUtility 클래스가 Dictionary를 직렬화 해주는 
 * 기능이 존재하지 않기 때문에 만든 커스텀 클래스 입니다. 
 */
[Serializable]
public class DataDictionary<TKey, TValue>
{
    public TKey key;
    public TValue value;
}

[Serializable]
public class JsonDataArray<TKey, TValue>
{
    public List<DataDictionary<TKey, TValue>> dataArray;
}

public static class DicJsonUtility
{
    public static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> dic, bool pretty = false) 
    {
        List<DataDictionary<TKey, TValue>> dataDictionaries = new();

        foreach (var pair in dic)
        { 
            DataDictionary<TKey, TValue> dataDictionary = new();
            dataDictionary.key = pair.Key;
            dataDictionary.value = pair.Value;
            dataDictionaries.Add(dataDictionary);
        }

        JsonDataArray<TKey, TValue> jsonDataArray = new();
        jsonDataArray.dataArray = dataDictionaries;

        return JsonUtility.ToJson(jsonDataArray, pretty);
    }
    public static Dictionary<TKey, TValue> FromJson<TKey, TValue>(string jsonData)
    {
        JsonDataArray<TKey, TValue> dataDictionaries = JsonUtility.FromJson<JsonDataArray<TKey, TValue>>(jsonData);

        // .. 결과로 반환할 딕셔너리를 생성합니다.
        Dictionary<TKey, TValue> dictionary = new();

        // .. JsonDataArray 배열의 요소를 순회하며 딕셔너리에 데이터를 추가합니다.
        foreach (var customDic in dataDictionaries.dataArray)
            dictionary.Add(customDic.key, customDic.value);

        return dictionary;
    }
}