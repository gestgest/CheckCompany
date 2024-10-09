using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GamePanelManager : PanelManager
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;

    protected override void Start()
    {
        base.Start();
           
        icon.sprite = panels[0].GetComponent<Panel>().GetSprite();
        this.title.text = panels[0].GetComponent<Panel>().GetTitle();
    }

    public override void SwitchingPanel(int index)
    {
        base.SwitchingPanel(index);
        
        icon.sprite = panels[index].GetComponent<Panel>().GetSprite();
        this.title.text = panels[index].GetComponent<Panel>().GetTitle();
    }
}
