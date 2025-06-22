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
        
        indexList.Clear();
        indexList.Add(0);
        SwitchingInfo(indexList);
    }

    public override void SwitchingPanelFromInt(int main_index)
    {
        base.SwitchingPanelFromInt(main_index);
        SwitchingInfo(indexList);
    }
    public override void SwitchingPanel(List<int> indexList)
    {
        Panel before_panel = GetPanel(this.indexList);
        if(!before_panel.GetHasMini())
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


    protected override void ClearNavStack()
    {
        base.ClearNavStack();
        //nav_panel_index_stack.Clear();
        UpdateNavButtonState();
    }

    protected override void Push_NavPanelStack(List<int> indexList)
    {
        Debug.Log("파카");
        base.Push_NavPanelStack(indexList);
        for (int i = 0; i < indexList.Count; i++)
        {
            Debug.Log("push의 index : " + indexList[i]);            
        }
        
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
