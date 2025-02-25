using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//를 상속받는 
public class PanelManager : MonoBehaviour
{
    public static PanelManager instance;
    [SerializeField] private Transform panel_parent;
    protected List<Panel> panels;

    Stack<int> nav_panel_stack;
    protected List<int> indexList;


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
        panels = new List<Panel>();
        indexList = new List<int>();

        //Panel 리스트에 넣어서 
        for (int i = 0; i < panel_parent.childCount; i++)
        {
            panels.Add(panel_parent.GetChild(i)
                .gameObject.GetComponent<Panel>()
            );
        }

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].gameObject.SetActive(false);
        }
        
        indexList.Add(0);
        panels[0].gameObject.SetActive(true);
    }
    public virtual void SwitchingPanelFromInt(int main_index)
    {
        indexList.Clear();
        indexList.Add(main_index);
        OnPanel(indexList);
    }

    //
    public virtual void SwitchingPanel(List<int> indexList)
    {
        //대충 panels에 들어가고
        OffPanel(this.indexList);
        SetIndexList(indexList);
        OnPanel(indexList);
    }

    public void OnPanel(List<int> indexList)
    {
        Panel panel = panels[indexList[0]];
        panel.gameObject.SetActive(true);

        for (int i = 1; i < indexList.Count; i++)
        {
            panel.SwitchingPanel(indexList[i]);
            panel = panel.GetPanel(indexList[i]);
        }
    }

    public void OffPanel(List<int> indexList)
    {
        //이거를 굳이?
        Panel panel = panels[indexList[0]];
        panel.OffPanel();
        
        
        for (int i = 1; i < indexList.Count; i++)
        {
            panel = panel.GetPanel(indexList[i]);
            panel.OffPanel();

            //tmp_panel = get뭐시기
        }
    }

    //direction가 1이면 오른쪽, -1이면 왼쪽
    public void NextPanel(int direction)
    {
        panels[indexList[0]].gameObject.SetActive(false);

        indexList[0] += direction;
        if (indexList[0] < 0)
        {
            indexList[0] += panels.Count;
        }
        indexList[0] %= panels.Count;

        panels[indexList[0]].gameObject.SetActive(true);
    }

    //뒤로가기 제외
    public void Click_Button_Panel(bool isNav, List<int> indexList)
    {
        if(isNav){
            Push_nav_panel_stack(indexList[0]);
        }
        SwitchingPanel(indexList);
    }


    public void Back_Nav_Panel()
    {
        int num = Pop_nav_panel_stack();
        //Debug.Log("pop의 index : " + num);
        
        indexList.Clear();
        indexList.Add(num);
        SwitchingPanel(indexList);
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



    void SetIndexList(List<int> indexList)
    {
        //메모리 참조 방지를 위해 얉은 복사
        if (indexList == this.indexList)
            return;

        (this.indexList).Clear();

        for (int i = 0; i < indexList.Count; i++)
        {
            this.indexList.Add(indexList[i]);
        }
    }


    public Panel GetPanel(List<int> indexList)
    {
        Panel panel = panels[indexList[0]];

        for (int i = 1; i < indexList.Count; i++)
        {
            panel = panel.GetPanel(indexList[i]);
        }

        return panel;
    }

    //
    void DebugList(List<int> indexList)
    {
        for (int i = 0; i < indexList.Count; i++)
        {
            Debug.Log(indexList[i]);
        }
    }
}
