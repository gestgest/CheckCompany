using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//assign - 1) assigned, 2) employee 
public class AssignEmployeeElement : MonoBehaviour
{
    protected Employee employee;
    protected bool isSelected;

    protected Image _icon;

    [SerializeField] protected IntEventChannelSO _RemoveRefEmployeeID; 

    // public override void SetEmployee(Employee employee, bool isSelected)
    // {
    //     base.SetEmployee(employee, isSelected);
    // }

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    public void SetEmployee(Employee employee, Sprite icon)
    {
        this.employee = employee;
        _icon.sprite = icon;
    }

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