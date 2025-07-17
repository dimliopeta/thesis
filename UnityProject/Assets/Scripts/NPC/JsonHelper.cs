using System;
using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(FixJson(json));
        return wrapper.Items;
    }

    private static string FixJson(string value)
    {
        return "{\"Items\":" + value + "}";
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T> { Items = array };
        return JsonUtility.ToJson(wrapper);
    }
    
}
