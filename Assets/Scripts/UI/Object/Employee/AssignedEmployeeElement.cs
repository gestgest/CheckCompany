using System;
using UnityEngine;
using UnityEngine.UI;


//selected
public class AssignedEmployeeElement : AssignEmployeeElement
{
    [SerializeField] private Sprite _nullIcon;
    //isSeleceted, _icon
    //false : null, true : employee
    

    public override void SwitchingIsSelcted()
    {
        //employee is not null
        if (isSelected)
        {
            _createMissionManager.RemoveRefEmployeeID(employee.ID);
            //IsSelected = false;
            //is => null
        }
        //if isSelected is false, nothing happen
    }

    //only UI
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
