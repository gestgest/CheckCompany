using UnityEngine;

public class AssignableEmployeeElement : AssignEmployeeElement
{
    private int selectedIndex;


    public override bool IsSelected
    {
        get { return isSelected; }
        set
        {
            base.IsSelected = value;

            if (isSelected)
            {
                _icon.color = Color.white;
                //Assigned에 넣기
            }
            else
            {
                _icon.color = Color.gray;
                //Assigned에 빼기
            }
        }
    }

}
