using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//놓는 물체
[CreateAssetMenu(fileName = "PanelSO", menuName = "ScriptableObject/PanelSO")]
public class PanelSO : ScriptableObject
{
    [SerializeField] private string title;
    [SerializeField] private Sprite icon;
    [SerializeField] private int index;
    [SerializeField] private PanelSO [] buttons;
    //bottom 버튼 SO들

    public string GetTitle(){
        return title;
    }
    public Sprite GetIcon(){
        return icon;
    }
    public int GetIndex(){
        return index;
    }
    public PanelSO [] GetButtons(){
        return buttons;
    }
}