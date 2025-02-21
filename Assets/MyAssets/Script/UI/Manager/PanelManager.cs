using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//를 상속받는 
public class PanelManager : MonoBehaviour
{
    public static PanelManager instance;
    [SerializeField] private Transform panel_parent;
    protected List<GameObject> panels;

    Stack<int> nav_panel_stack;
    protected int main_index;
    protected int sub_index = -1; //안 쓰면 -1
    protected int mini_index = -1; //안 쓰면 -1


    // => 다른 싱글톤처럼 new로 하면 MonoBehaviour와 같은 클래스가 문제를 일으킬 수 있다.
    //유니티식 싱글톤
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
        
        //return instance;
    }

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
        main_index = 0;
        panels[main_index].SetActive(true);
    }

    //버튼용
    public void SwitchPanelFromButton(int index)
    {
        SwitchingPanel(index);
    }
    //
    public virtual void SwitchingPanel(int main_index, int sub_index = -1, int mini_index = -1)
    {
        //대충 panels에 들어가고
        OffPanel(main_index, sub_index, mini_index);
        OnPanel(main_index, sub_index, mini_index);

        this.main_index = main_index;
    }

    public void OnPanel(int main_index, int sub_index , int mini_index)
    {
        panels[main_index].SetActive(true);

    }

    public void OffPanel(int main_index, int sub_index, int mini_index)
    {
        panels[this.main_index].SetActive(false);
        //panels.GetComponent<Panel>().SetSub
    }

    //direction가 1이면 오른쪽, -1이면 왼쪽
    public void NextPanel(int direction)
    {
        panels[main_index].SetActive(false);

        main_index += direction;
        if (main_index < 0)
        {
            main_index += panels.Count;
        }
        main_index %= panels.Count;

        panels[main_index].SetActive(true);
    }

    //뒤로가기 제외
    public void Click_Button_Panel(bool isNav, int main_index, int sub_index = -1, int mini_index = -1)
    {
        if(isNav){
            Push_nav_panel_stack(main_index);
        }
        SwitchingPanel(main_index, sub_index, mini_index);
    }


    public void Back_Nav_Panel()
    {
        int num = Pop_nav_panel_stack();
        //Debug.Log("pop의 index : " + num);
        
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
        //Debug.Log("push의 index : " + index);
        nav_panel_stack.Push(index);
    }
    //




    public Panel GetPanel(int main_index, int sub_index = -1, int mini_index = -1)
    {
        Panel panel = panels[main_index].GetComponent<Panel>(); //메인

        if (sub_index == -1) 
        {
            return panel;
        }
        MainPanel mainPanel = panel as MainPanel;
        
        panel = mainPanel.GetPanel(sub_index); //서브
        if (mini_index == -1)
        {
            return panel;
        }
        
        SubPanel subPanel = panel as SubPanel;
        panel = subPanel.GetPanel(mini_index); //미니

        return panel;
    }
    //
}
