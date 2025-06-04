using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GetStringEventChannelSO", menuName = "ScriptableObject/Event/GetStringEventChannelSO")]
public class GetStringEventChannelSO : ScriptableObject
{
    public Func<string> _onEventRaised;

    public string RaiseEvent()
    {
        return _onEventRaised.Invoke();
    }
}
