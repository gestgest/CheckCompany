using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//놓는 물체
[CreateAssetMenu(fileName = "PanelSO", menuName = "ScriptableObject/PanelSO")]
public class PanelSO : ScriptableObject
{
    [SerializeField] private string title;
    [SerializeField] private Sprite icon;

    public string GetTitle(){
        return title;
    }
    public Sprite GetIcon(){
        return icon;
    }
}