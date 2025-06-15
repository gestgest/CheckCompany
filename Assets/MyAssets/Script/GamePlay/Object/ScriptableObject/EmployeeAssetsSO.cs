using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeAssetsSO", menuName = "ScriptableObject/EmployeeAssetsSO")]
public class EmployeeAssetsSO : ScriptableObject
{
    [SerializeField] private EmployeeAsset[] _assets;
    
    
}
