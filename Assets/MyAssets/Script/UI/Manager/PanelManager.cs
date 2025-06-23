using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//UIManager를 맨 앞에.
//그거를 상속받는 UIMenuManager, UIGameManager
public class PanelManager : MonoBehaviour
{
    public static PanelManager instance;
    [SerializeField] private Transform panel_parent;
    protected List<Panel> panels;

    protected Stack<List<int>> nav_panel_index_stack;
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
        //임시방편
        Destroy(instance);
        instance = this;
        //return instance;
    }

    protected virtual void Start()
    {
        nav_panel_index_stack = new Stack<List<int>>();
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
        panels[0].OnPanel();
    }

    //메인 버튼 Panel 이동
    public virtual void SwitchingPanelFromInt(int main_index)
    {
        OffPanel(indexList);
        indexList.Clear();
        indexList.Add(main_index);
        OnPanel(indexList);
        ClearNavStack();
    }

    //
    public virtual void SwitchingPanel(List<int> indexList)
    {
        //대충 panels에 들어가고
        OffPanel(this.indexList);
        this.indexList = indexList;
        OnPanel(indexList);
    }

    public void OnPanel(List<int> indexList)
    {
        Panel panel = panels[indexList[0]];
        panel.OnPanel();

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

    //뒤로가기 제외 => Panel 이동, subPanel끼리 이동할 경우에만
    public void SwitchingSubPanel(bool isNav, List<int> indexList)
    {
        if(isNav){
            Push_NavPanelStack(this.indexList); //이전 panel 값 nav 저장
        }
        SwitchingPanel(indexList);
    }


    //뒤로가기 버튼
    public void Back_Nav_Panel()
    {
        OffPanel(indexList);
        
        //원래 없어도 되지만
        if (nav_panel_index_stack.Count == 0)
        {
            Debug.Log("이 메세지가 나오면 안됨");
            return;
        }
        List<int> output = Pop_NavPanelStack();
        
        //set beforeIndex
        indexList = output;
        SwitchingPanel(indexList);
    }


    #region PROPERTY
    public List<int> GetIndexList()
    {
        return indexList;
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

    #endregion



    #region STACK
    public virtual void ClearNavStack()
    {
        nav_panel_index_stack.Clear();
        //NavButtonSwitching();
    }

    protected virtual void Push_NavPanelStack(List<int> indexList)
    {
        nav_panel_index_stack.Push(indexList);
        //NavButtonSwitching();
    }

    protected virtual List<int> Pop_NavPanelStack()
    {
        return nav_panel_index_stack.Pop();
    }
    #endregion
}
