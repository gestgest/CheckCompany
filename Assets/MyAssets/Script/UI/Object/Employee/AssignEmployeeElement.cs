using System;
using UnityEngine;
using UnityEngine.UI;

//assign - 1) assigned, 2) employee 
public class AssignEmployeeElement : MonoBehaviour
{
    protected Employee employee;
    private Image icon;
    protected bool isSelected = false;

    public virtual void SetEmployee(Employee employee, bool isSelected)
    {
        this.employee = employee;
        
        //add icon
        IsSelected = isSelected;
    }

    //button Fucntion
    public virtual void SwitchingIsSelcted()
    {
        IsSelected = !IsSelected;
    }
    public virtual bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            //override
        }
    }

}