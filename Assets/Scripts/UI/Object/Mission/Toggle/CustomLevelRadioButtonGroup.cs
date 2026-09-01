using UnityEngine;

public class CustomLevelRadioButtonGroup : CustomRadioButtonGroup
{
    public override void SetToggle(int index)
    {
        base.SetToggle(index);
        _createMissionManager.SetLevel(index);
    }

}
