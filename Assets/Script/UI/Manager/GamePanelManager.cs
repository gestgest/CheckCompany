using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePanelManager : PanelManager
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject [] buttons;


    const int POOL_MAX_SIZE = 5;
    protected override void Start()
    {
        base.Start();
           
        SwitchingInfo(0);
    }

    public override void SwitchingPanel(int index)
    {
        base.SwitchingPanel(index);
        
        SwitchingInfo(index);
    }

    void SwitchingInfo(int index) {
        Panel panel = panels[index].GetComponent<Panel>();
        //top 정보 수정
        icon.sprite = panel.GetSprite();
        this.title.text = panel.GetTitle();

        //bottom 정보 수정
        PanelSO [] panelInfos = panel.GetButtons();
        for(int i = 0; i < panelInfos.Length; i++)
        {
            buttons[i].SetActive(true);

            buttons[i].transform.GetChild(0).GetComponent<Image>().sprite = panelInfos[i].GetIcon();
            //클릭하면 이벤트 추가
        }
        
        for(int i = panelInfos.Length; i < POOL_MAX_SIZE; i++)
        {
            buttons[i].SetActive(false);
        }
    }
}
