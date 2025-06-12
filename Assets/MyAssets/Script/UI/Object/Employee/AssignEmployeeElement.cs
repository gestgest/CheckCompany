using System;
using UnityEngine;
using UnityEngine.UI;

public class AssignEmployeeElement : MonoBehaviour
{
    private Employee employee;
    private Image icon;
    private bool isSelected = false;
    private SelctedType _selctedType;

    void SetEmployee(Employee employee, SelctedType selctedType)
    {
        this.employee = employee;
        
        //add icon
        this._selctedType = selctedType;
    }

    //button Fucntion
    public void SwitchingIsSelcted()
    {
        IsSelected = !IsSelected;
    }
    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            //true : 회색, false : 그대로
            //뒤집히는 animation?
        }
    }

    public enum SelctedType
    {
        NULL = 0,
        DEFAULT = 1,
        SELECTED = 2,
    }

}