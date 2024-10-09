using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//를 상속받는 
public class PanelManager : MonoBehaviour
{
    [SerializeField] private Transform panel_parent;
    protected List<GameObject> panels;
    int set_index;

    protected virtual void Start()
    {
        panels = new List<GameObject>();

        for (int i = 0; i < panel_parent.childCount; i++)
        {
            panels.Add(panel_parent.GetChild(i).gameObject);
        }

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }
        set_index = 0;
        panels[set_index].SetActive(true);
    }

    virtual public void SwitchingPanel(int index)
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
        if (set_index < 0)
        {
            set_index += panels.Count;
        }
        set_index %= panels.Count;

        panels[set_index].SetActive(true);
    }
    //
}
