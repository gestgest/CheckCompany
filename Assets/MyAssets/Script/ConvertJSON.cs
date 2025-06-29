using System;
using System.Collections.Generic;
using UnityEngine;


public static class ConvertJSON
{
    //if key not find, return defaultValue
    public static T SafeGet<T>(Dictionary<string, object> data, string key, T defaultValue)
    {
        if (!data.TryGetValue(key, out object val))
        {
            return defaultValue;
        }
        if (data[key] == null)
        {
            return defaultValue;
        }
        try
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}