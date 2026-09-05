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

        //자식이 있으면 0번 자식까지 들어간 것으로 경로를 잡는다.
        //단 hasMini 패널은 예외다 - 그 자식은 버튼을 눌러야 열리는 미니(모달) 패널이라
        //Panel.OnPanel()이 일부러 SwitchingPanel(0)을 건너뛴다. 여기서 0을 붙이면
        //"화면은 안 열렸는데 경로만 열린 척"하는 상태가 되고, 그 뒤 PushIndexList(0)가
        //이미 그 미니 패널을 현재 위치로 보고 "자식 수 = 0"이라며 전환을 취소해버린다.
        //(CreateMissionPanel에서 직원 배정 패널이 안 열리던 원인)
        Panel opened = GetPanel(indexList);

        if (opened != null && opened.GetPanels().Length != 0 && !opened.GetHasMini())
        {
            indexList.Add(0);
        }

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
        // for (int i = 0; i < this.indexList.Count; i++)
        // {
        //     Debug.Log("네비의 index["+ i + "] : "  + this.indexList[i]);            
        // }
        
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

    public void PopIndexList()
    {
        GetPanel(indexList).OffPanel();
        indexList.RemoveAt(indexList.Count - 1);
    }
    public void PushIndexList(int value)
    {
        //GetPanel(indexList).OffPanel();
        Panel current = GetPanel(indexList);

        //전환에 실패했는데도 indexList에 추가해버리면 경로가 오염되고,
        //다음 호출에서 없는 경로를 걷다가 예외가 난다. 반드시 먼저 검증할 것.
        if (current == null || value < 0 || value >= current.GetPanels().Length)
        {
            Debug.LogWarning(
                $"PushIndexList({value}) 취소됨. 현재 패널 = '{(current == null ? "null" : current.name)}', " +
                $"자식 수 = {(current == null ? 0 : current.GetPanels().Length)}.", current);
            return;
        }

        current.SwitchingPanel(value);
        indexList.Add(value);
    }
    public void SwitchingIndexList(int value)
    {
        indexList.RemoveAt(indexList.Count - 1);
        GetPanel(indexList).SwitchingPanel(value);
        indexList.Add(value);
    }


    #region PROPERTY
    public List<int> GetIndexList()
    {
        return indexList;
    }


    /// <summary>indexList 경로를 따라 내려간다. 경로가 끊기면 마지막으로 유효했던 패널을 돌려준다.</summary>
    public Panel GetPanel(List<int> indexList)
    {
        if (indexList == null || indexList.Count == 0)
        {
            Debug.LogWarning("GetPanel: indexList가 비어있음.");
            return null;
        }

        if (indexList[0] < 0 || indexList[0] >= panels.Count)
        {
            Debug.LogWarning($"GetPanel: 루트 index {indexList[0]}가 범위 밖. panels 크기 = {panels.Count}.");
            return null;
        }

        Panel panel = panels[indexList[0]];

        for (int i = 1; i < indexList.Count; i++)
        {
            Panel child = panel.GetPanel(indexList[i]);

            //경로가 실제 패널 트리와 어긋난 상태. 여기서 더 내려가면 예외가 난다.
            if (child == null)
            {
                Debug.LogWarning(
                    $"GetPanel: 경로 [{string.Join(",", indexList)}]의 {i}번째에서 끊김. " +
                    $"'{panel.name}'에 자식 {indexList[i]}가 없음. indexList가 실제 화면과 어긋나 있음.", panel);
                return panel;
            }

            panel = child;
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
