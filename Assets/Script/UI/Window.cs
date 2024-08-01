using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    [SerializeField] private GameObject [] panels;
    int set_index = 0;

    private void Start()
    {
        for(int i = 0; i < panels.Length; i++)
        {
            panels[set_index].SetActive(false);
        }
        panels[set_index].SetActive(true);
    }


    public void SwitchingPanel(int index)
    {
        panels[set_index].SetActive(false);
        panels[index].SetActive(true);
        set_index = index;
    }
}
