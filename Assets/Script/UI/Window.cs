using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//를 상속받는 
public class Window : MonoBehaviour
{
    [SerializeField] private GameObject [] panels;
    int set_index = 0;

    private void Start()
    {
        for(int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
        panels[set_index].SetActive(true);
    }


    public void SwitchingPanel(int index)
    {
        panels[set_index].SetActive(false);
        panels[index].SetActive(true);
        set_index = index;
    }

    //direction가 1이면 오른쪽, -1이면 왼쪽
    public void NextPanel(int direction)
    {
        panels[set_index].SetActive(false);
        
        set_index += direction;
        if(set_index < 0){
            set_index += panels.Length;
        }
        set_index %= panels.Length;

        panels[set_index].SetActive(true);
    }

    //
}
