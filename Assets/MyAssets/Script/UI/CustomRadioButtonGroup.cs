using System.Collections.Generic;
using UnityEngine;
using Toggle = UnityEngine.UI.Toggle;

public class CustomRadioButtonGroup : MonoBehaviour
{
    [SerializeField] protected CreateMissionManagerSO _createMissionManager;

    private List<Toggle> toggles;

    private int set_index;
    private bool isLock = false;

    private void Awake()
    {
        toggles = new List<Toggle>();
        for (int i = 0; i < transform.childCount; i++)
            toggles.Add(transform.GetChild(i).GetComponent<Toggle>());
    }

    public List<Toggle> GetToggles()
    {
        //protected
        return toggles;
    }
    
    
    

    //toggle event
    public virtual void SetToggle(int index)
    {
        if (isLock)
            return;

        isLock = true;
        toggles[this.set_index].isOn = false;
        isLock = false;

        //SetAllTogglesOff();
        //toggles[index].isOn = true;
        this.set_index = index;
    }

    public void SetIndex(int index)
    {
        isLock = true;
        SetAllTogglesOff();
        this.set_index = index;
        toggles[index].isOn = true;
        isLock = false;
    }

    private void SetAllTogglesOff()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].isOn = false;
        }
    }

    public int GetIndex()
    {
        return set_index;
    }
}