using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GamePanelManager : PanelManager
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Animator panel_animator;

    bool isExpandPanel = false;

    const int POOL_MAX_SIZE = 5;
    protected override void Start()
    {
        base.Start();
        
        SwitchingInfo(0);
    }

    public override void SwitchingPanel(int index)
    {
        SwitchingInfo(index);
        base.SwitchingPanel(index);
    }

    void SwitchingInfo(int index)
    {
        Panel panel = panels[index].GetComponent<Panel>();
        //top 정보 수정
        icon.sprite = panel.GetSprite();
        this.title.text = panel.GetTitle();
        int parent_height = panels[set_index].GetComponent<Panel>().Get_panel_height();
        int height = panel.Get_panel_height();
        
        if(parent_height == height && height != 0) {
            return;
        }
        //bottom 버튼 정보 수정 [이미지]
        PanelSO[] panelInfos = panel.GetButtons();

        for (int i = 0; i < panelInfos.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);

            buttons[i].transform.GetChild(0).GetComponent<Image>().sprite = panelInfos[i].GetIcon();
            int panel_index = panelInfos[i].GetIndex();

            bool isNav = height < panelInfos[i].GetHeight();
            //클릭하면 이벤트 추가
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => { Click_Button_Panel(panel_index, isNav); });
        }

        for (int i = panelInfos.Length; i < POOL_MAX_SIZE; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }

    }

    public void TransformPanel()
    {
        panel_animator.SetTrigger("isExpand");
    }


}
