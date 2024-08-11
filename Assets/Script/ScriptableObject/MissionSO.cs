using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionSO", menuName = "ScriptableObject/MissionSO")]
public class MissionSO : ScriptableObject
{
    [SerializeField] private MissionType mission_type;
    [SerializeField] private string[] missions;
}
