using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//를 상속받는 
public class PanelManager : MonoBehaviour
{
    [SerializeField] private Transform panel_parent;
    protected List<GameObject> panels;

    Stack<int> nav_panel_stack;
    protected int set_index;

    protected virtual void Start()
    {
        nav_panel_stack = new Stack<int>();
        panels = new List<GameObject>();


        //Panel 리스트에 넣어서 
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

    public virtual void SwitchingPanel(int index)
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

    //뒤로가기 제외
    public void Click_Button_Panel(int index, bool isNav)
    {
        if(isNav){
            Push_nav_panel_stack(set_index);
        }
        SwitchingPanel(index);
    }

    public void Back_Nav_Panel()
    {
        int num = Pop_nav_panel_stack();
        Debug.Log("pop의 index : " + num);
        
        SwitchingPanel(num);
    }

    protected int Pop_nav_panel_stack()
    {
        if(nav_panel_stack.Count == 0)
            return 0;
        return nav_panel_stack.Pop();
    }

    protected void Push_nav_panel_stack(int index)
    {
        Debug.Log("push의 index : " + index);
        nav_panel_stack.Push(index);
    }
    //
}
