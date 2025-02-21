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
        
        SwitchingInfo(0);
    }

    public override void SwitchingPanel(int main_index, int sub_index = -1, int mini_index = -1)
    {
        SwitchingInfo(main_index);
        base.SwitchingPanel(main_index, sub_index, mini_index);
    }


    //Panel 정보 수정 => panel만 수정
    void SwitchingInfo(int index)
    {
        //panel 
        Panel panel = panels[index].GetComponent<Panel>();

        //top 정보 수정
        icon.sprite = panel.GetSprite();
        this.title.text = panel.GetTitle();



        //이미지
        //if(parent_height == height && height != 0) {
        //    return;
        //}

        //네비게이션 스택 제거
    }

    public void TransformPanel()
    {
        panel_animator.SetTrigger("isExpand");
    }


}
