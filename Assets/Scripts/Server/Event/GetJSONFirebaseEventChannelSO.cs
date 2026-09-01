using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

//string 매개변수가 3개고 반환값
[CreateAssetMenu(fileName = "GetJSONFirebaseEventChannelSO", menuName = "ScriptableObject/Event/GetJSONFirebaseEventChannelSO")]
public class GetJSONFirebaseEventChannelSO : ScriptableObject
{
    //반환값이 맨 뒤
    public Func<string, string, string, Task<object>> _onEventRaised;

    public Task<object> RaiseEvent(string collection_name, string id, string key)
    {
        return _onEventRaised.Invoke(collection_name, id, key);
    }
}