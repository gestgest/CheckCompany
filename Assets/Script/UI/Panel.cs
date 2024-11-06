using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    [SerializeField] private PanelSO panelInfo;

    //UI컴포넌트 버튼이나 인풋 field
    private Transform selected_parent; //부모 오브젝트
    private List<Selectable> selected_objects; //선택 오브젝트들

    int index;

    void Start()
    {
        selected_objects = new List<Selectable>();
        selected_parent = transform;
        
        for(int i = 0; i < selected_parent.childCount; i++)
        {
            Selectable sel = selected_parent.GetChild(i).GetComponent<Selectable>();
            if(sel == null){
                continue;
            }
            // 클릭 이벤트를 추가하기 위해 EventTrigger 컴포넌트를 동적으로 추가합니다.
            EventTrigger trigger = sel.gameObject.AddComponent<EventTrigger>();

            // 클릭 이벤트 항목을 생성합니다.
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;

            int select_index = selected_objects.Count;
            // 클릭 시 호출할 메서드를 설정합니다.
            entry.callback.AddListener((eventData) => { GetSelectIndex(sel, select_index); });

            selected_objects.Add(sel);
            // EventTrigger에 클릭 이벤트 항목을 추가합니다.
            trigger.triggers.Add(entry);
        }
        index = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Index++;
        }
    }

    public int Index
    {
        get { return index; }
        set {
            
            index = value;
            Debug.Log("Panel 함수 : 끼루루" + index);
            if (selected_objects.Count == 0)
            {
                return;
            }
            //out of bound 해결
            if(selected_objects.Count <= index){
                index -= selected_objects.Count;
            }
            selected_objects[index].Select();
        }
    }

    public void GetSelectIndex(Selectable selectable, int i)
    {
        Index = i;
    }

    public Sprite GetSprite()
    {
        return panelInfo.GetIcon();
    }
    public string GetTitle()
    {
        return panelInfo.GetTitle();
    }
    public PanelSO [] GetButtons()
    {
        return panelInfo.GetButtons();
    }
    public int Get_panel_index()
    {
        return panelInfo.GetIndex();
    }
    public int Get_panel_height()
    {
        return panelInfo.GetHeight();
    }
}
