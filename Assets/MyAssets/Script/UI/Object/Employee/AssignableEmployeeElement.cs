using UnityEngine;

public class AssignableEmployeeElement : AssignEmployeeElement
{
    private int selectedIndex;

    //_RemoveRefEmployeeIndex
    [SerializeField] private IntEventChannelSO _AddRefEmployeeID;
    
    public override bool IsSelected
    {
        get { return isSelected; }
        set
        {
            base.IsSelected = value;

            if (isSelected)
            {
                _icon.color = Color.white;
                _AddRefEmployeeID.RaiseEvent(employee.ID);
                //Assigned에 넣기
            }
            else
            {
                _icon.color = Color.gray;
                _RemoveRefEmployeeID.RaiseEvent(employee.ID);
                //Assigned에 빼기
            }
        }
    }

}
