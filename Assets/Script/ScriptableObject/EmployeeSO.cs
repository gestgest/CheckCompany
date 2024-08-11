using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeSO", menuName = "ScriptableObject/EmployeeSO")]
public class EmployeeSO : ScriptableObject
{
    [SerializeField] private EmployeeType employeeType;
    [SerializeField] private Sprite icon;
}
