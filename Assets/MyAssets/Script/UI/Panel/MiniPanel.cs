using System;
using Unity.VisualScripting;
using UnityEngine;

public class MiniPanel : Panel
{
    [SerializeField] private GameObject background;
    

    void Awake()
    {
        //background = transform.parent.gameObject;
        hasMini = true;
    }
    public override void SwitchingPanel(int index)
    {
        background.SetActive(true);
        //background.SetActive(true);
    }
    public override void OnPanel()
    {
        base.OnPanel();
        background.SetActive(true);
    }

    //이걸로 온 오프 하지마라
    public override void OffPanel()
    {
        base.OffPanel();
        background.SetActive(false);
    }
    
}
