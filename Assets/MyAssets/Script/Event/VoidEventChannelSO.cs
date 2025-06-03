using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEventChannelSO", menuName = "ScriptableObject/Event/VoidEventChannelSO")]
public class VoidEventChannelSO : ScriptableObject
{
    public UnityAction _onEventRaised;

    public void RaiseEvent()
    {
        if (_onEventRaised != null)
        {
            _onEventRaised.Invoke();
        }
        else
        {
            Debug.LogError("대충 오류라는 내용");
        }
    }
}
