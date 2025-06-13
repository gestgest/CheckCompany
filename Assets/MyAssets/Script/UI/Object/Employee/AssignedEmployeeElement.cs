using System;
using UnityEngine;
using UnityEngine.UI;


//selected
public class AssignedEmployeeElement : AssignEmployeeElement
{
    [SerializeField] private Sprite _nullIcon;
    //isSeleceted
    //false : null, true : employee
    
    private Image _icon;

    
    // public override void SetEmployee(Employee employee, bool isSelected)
    // {
    //     base.SetEmployee(employee, isSelected);
    // }
    
    private void Start()
    {
        _icon = GetComponent<Image>();
    }

    public override void SwitchingIsSelcted()
    {
        if (isSelected)
        {
            IsSelected = false;
        }
        //if isSelected is false, nothing happen
        
    }

    public override bool IsSelected
    {
        get { return isSelected; }
        set
        {
            base.IsSelected = value;
            
            if (isSelected)
            {
                //_icon.sprite = employee;
            }
            else //null
            {
                _icon.sprite = _nullIcon;
            }
        }
    }
}
