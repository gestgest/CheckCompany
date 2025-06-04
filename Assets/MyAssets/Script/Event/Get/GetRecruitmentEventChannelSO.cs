using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GetRecruitmentEventChannelSO", menuName = "ScriptableObject/Event/GetRecruitmentEventChannelSO")]

public class GetRecruitmentEventChannelSO : ScriptableObject
{
    public Func<Recruitment> _onEventRaised;

    public Recruitment RaiseEvent()
    {
        return _onEventRaised.Invoke();
    }
}
