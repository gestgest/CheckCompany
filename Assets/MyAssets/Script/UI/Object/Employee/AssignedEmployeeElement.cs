using System;
using UnityEngine;
using UnityEngine.UI;


//selected
public class AssignedEmployeeElement : AssignEmployeeElement
{
    [SerializeField] private Sprite _nullIcon;
    //isSeleceted
    //false : null, true : employee
    

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
