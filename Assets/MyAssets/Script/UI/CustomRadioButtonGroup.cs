using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

public class CustomRadioButtonGroup : MonoBehaviour
{
    [SerializeField] private List<Toggle> toggles = new List<Toggle>();

    private void Awake()
    {
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
        SetAllTogglesOff();
        toggles[index].isOn = true;
    }

    private void SetAllTogglesOff()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].isOn = false;
        }
    }
}
