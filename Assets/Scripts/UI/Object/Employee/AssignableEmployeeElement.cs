using UnityEngine;

public class AssignableEmployeeElement : AssignEmployeeElement
{
    private int selectedIndex;

    public override void SwitchingIsSelcted()
    {
        if (!isSelected)
        {
            //if assigned employee max, return
            if (_createMissionManager.GetAssignableEmployeeSize() == _createMissionManager.GetRefEmployeesID().Count)
            {
                return;
            }

            _createMissionManager.AddRefEmployeeID(employee.ID);
        }
        else
        {
            _createMissionManager.RemoveRefEmployeeID(employee.ID);
            //Assigned에 빼기
        }
    }

    //only UI
    public override bool IsSelected
    {
        get { return isSelected; }
        set
        {
            base.IsSelected = value;
            if(isSelected)
            {
                _icon.color = Color.HSVToRGB(0.25f, 0.25f, 0.25f);
            }
            else
            {
                _icon.color = Color.white;
            }
        }
    }

}
