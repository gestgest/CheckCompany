using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] private PanelSO panelInfo;
    [SerializeField] protected Panel[] panels;

    //UI컴포넌트 버튼이나 인풋 field
    private Transform selected_parent; //부모 오브젝트
    private List<Selectable> selected_objects; //선택 오브젝트들

    private int select_index;
    int panel_index = -1;
    [SerializeField] protected bool hasMini;

    protected virtual void Start()
    {
        CloseAllPanels();
        selected_objects = new List<Selectable>();
        selected_parent = transform;

        //버튼 같은 선택가능한 오브젝트
        for (int i = 0; i < selected_parent.childCount; i++)
        {
            Selectable sel = selected_parent.GetChild(i).GetComponent<Selectable>();
            if (sel == null)
            {
                continue;
            }

            // 클릭 이벤트를 추가하기 위해 EventTrigger 컴포넌트를 동적으로 추가합니다.
            EventTrigger trigger = sel.gameObject.AddComponent<EventTrigger>();

            // 클릭 이벤트 항목을 생성합니다.
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;

            // 클릭 시 호출할 메서드를 설정합니다.
            entry.callback.AddListener((eventData) => { GetSelectIndex(sel, select_index); });

            selected_objects.Add(sel);
            // EventTrigger에 클릭 이벤트 항목을 추가합니다.
            trigger.triggers.Add(entry);
        }
        
        select_index = selected_objects.Count; //일부로 소문자로 함
        panel_index = -1;
        if (!hasMini)
            SwitchingPanel(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Select_Index++;
        }
    }

    //자식 index 온
    public virtual void SwitchingPanel(int index)
    {
        if (this.panel_index != -1)
        {
            //Debug.Log(this.index);
            panels[this.panel_index].OffPanel();
        }

        if (index >= panels.Length)
        {
            return;
        }

        this.panel_index = index;
        panels[index].OnPanel();
        
    }
    public virtual void CloseAllPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].OffPanel();
        }
    }

    public virtual void OnPanel()
    {
        gameObject.SetActive(true);
        //나중에 이거 바꿔야함 hasMini가 아닌 자식의 objectType
        if (!hasMini)
            SwitchingPanel(0); //무조건 Panel 초기 넣자.
    }

    public virtual void OffPanel()
    {
        if (this.panel_index != -1)
            panels[this.panel_index].OffPanel();
        gameObject.SetActive(false);
    }

    #region PROPERTY

    public int Select_Index
    {
        get { return select_index; }
        set
        {
            select_index = value;
            if (selected_objects.Count == 0)
            {
                return;
            }

            //out of bound 해결
            if (selected_objects.Count <= select_index)
            {
                select_index -= selected_objects.Count;
            }
            selected_objects[select_index].Select();
        }
    }

    public void GetSelectIndex(Selectable selectable, int i)
    {
        Select_Index = i;
    }

    public Sprite GetSprite()
    {
        return panelInfo.GetIcon();
    }

    public string GetTitle()
    {
        return panelInfo.GetTitle();
    }

    public Panel[] GetPanels()
    {
        return panels;
    }

    public Panel GetPanel(int index)
    {
        return panels[index];
    }

    public bool GetHasMini()
    {
        return hasMini;
    }

    #endregion
}