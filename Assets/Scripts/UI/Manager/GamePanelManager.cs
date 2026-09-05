using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class GamePanelManager : PanelManager
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Animator panel_animator;
    [SerializeField] private Button [] bottom_buttons;

    [SerializeField] private Button backButton;

    const int POOL_MAX_SIZE = 5;

    protected override void Start()
    {
        base.Start();

        for(int i = 0; i < bottom_buttons.Length; i++)
        {
            Panel panel = panels[i].GetComponent<Panel>();

            //버튼 이미지설정
            Transform button_transform = bottom_buttons[i].transform.GetChild(0);
            button_transform.GetComponent<Image>().sprite = panel.GetSprite();
            //panel 
        }
        
        indexList.Add(0);
        SwitchingInfo(indexList);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("스택 : " + nav_panel_index_stack.Count);            

            for (int i = 0; i < this.indexList.Count; i++)
            {
                Debug.Log("현재 위치 index["+ i + "] : "  + indexList[i]);            
            }

        }
    }

    public override void SwitchingPanelFromInt(int main_index)
    {
        base.SwitchingPanelFromInt(main_index);
        SwitchingInfo(indexList);
    }
    public override void SwitchingPanel(List<int> indexList)
    {
        //헤더(아이콘/제목)는 "어디로 가는지"만 보고 정한다.
        //예전에는 떠나는 패널의 hasMini를 보고 건너뛰었는데, 그 값은 "이 패널이 미니 자식을 갖고 있다"는
        //뜻이라 목적지와 아무 상관이 없다. 그래서 indexList가 미니 패널까지 내려가 있느냐에 따라
        //같은 이동인데도 헤더가 갱신되기도, 안 되기도 했다.
        //(미니 패널은 PushIndexList로 여는데 그쪽은 헤더를 아예 안 건드리므로,
        // 여기서 무조건 갱신해도 모달 제목이 헤더에 올라오지는 않는다)
        SwitchingInfo(indexList);

        base.SwitchingPanel(indexList);
    }


    //Panel 정보 수정 => panel만 수정
    void SwitchingInfo(List<int> indexList)
    {
        //panel 
        Panel panel = GetPanel(indexList);

        //top 정보 수정
        icon.sprite = panel.GetSprite();
        this.title.text = panel.GetTitle();
        
    }

    public void TransformPanel()
    {
        panel_animator.SetTrigger("isExpand");
    }

    /// <summary> stack 갯수에 따라 On/OFF </summary>
    private void UpdateNavButtonState()
    {
        if (nav_panel_index_stack.Count == 0)
        {
            //대충 네비 버튼 비활성화
            backButton.gameObject.SetActive(false);
        }
        else
        {
            //대충 네비 버튼 활성화
            backButton.gameObject.SetActive(true);
        }
    }

    #region STACK


    public override void ClearNavStack()
    {
        base.ClearNavStack();
        //nav_panel_index_stack.Clear();
        UpdateNavButtonState();
    }

    protected override void Push_NavPanelStack(List<int> indexList)
    {
        base.Push_NavPanelStack(indexList);
        
        //nav_panel_index_stack.Push(indexList);
        UpdateNavButtonState();
    }

    protected override List<int> Pop_NavPanelStack()
    {
        List<int> result = base.Pop_NavPanelStack();
        UpdateNavButtonState();
        return result;
    }
    #endregion

}
