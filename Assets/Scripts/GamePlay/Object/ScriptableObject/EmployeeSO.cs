using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeSO", menuName = "ScriptableObject/EmployeeSO")]
public class EmployeeSO : ScriptableObject
{
    [SerializeField] private EmployeeType employeeType;
    [SerializeField] private Sprite icon;

    public Sprite GetIcon()
    {
        return icon;
    }
    public EmployeeType GetEmployeeType()
    {
        return employeeType;
    }

    public void SetIcon(Sprite icon)
    {
        this.icon = icon;
    }
    public void SetEmployeeType(EmployeeType et)
    {
        employeeType = et;
    }
}
