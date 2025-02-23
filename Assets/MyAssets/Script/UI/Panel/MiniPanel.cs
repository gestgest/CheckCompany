using System;
using Unity.VisualScripting;
using UnityEngine;

public class MiniPanel : Panel
{
    private GameObject background;
    

    void Awake()
    {
        background = transform.parent.gameObject;
        hasMini = true;
    }
    protected override void OnEnable()
    {
        
        background.SetActive(true);
    }

    protected void OnDisable()
    {
        background.SetActive(false);
    }

    public override void SwitchingPanel(int index)
    {
        //background.SetActive(true);
    }
}
