using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

public class CustomRadioButtonGroup : MonoBehaviour
{
    [SerializeField] private List<Toggle> toggles;

    private int set_index;
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

    public void SetToggle(int index)
    {
        if (this.set_index == index)
        {
            if (!toggles[set_index].isOn)
            {
                toggles[set_index].isOn = true;
            }
            return;
        }

        toggles[this.set_index].isOn = false;
        //SetAllTogglesOff();
        //toggles[index].isOn = true;
        this.set_index = index;
    }
    public void SetIndex(int index)
    {
        SetAllTogglesOff();
        this.set_index = index;
        toggles[index].isOn = true;
    }

    private void SetAllTogglesOff()
    {
        Debug.Log("엄준식");
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
