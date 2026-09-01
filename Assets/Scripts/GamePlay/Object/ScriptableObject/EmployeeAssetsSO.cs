using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeAssetsSO", menuName = "ScriptableObject/EmployeeAssetsSO")]
public class EmployeeAssetsSO : ScriptableObject
{
    //id == index
    [SerializeField] private EmployeeAsset[] _assets;
    
    public EmployeeAsset GetAsset(int index)
    {
        return _assets[index];
    }
}
