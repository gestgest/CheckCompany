using UnityEngine;

public class CustomTypeRadioButtonGroup : CustomRadioButtonGroup
{
    public override void SetToggle(int index)
    {
        base.SetToggle(index);
        _createMissionManager.SetEmployeeType(index);
    }
}
