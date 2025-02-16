using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MultiLayoutGroup : MonoBehaviour
{
    //여러가지 gameobject 그룹들 => transform
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private RectTransform parentRectTransform;
    
    private float before_pos_h = 0;
    private int changeCount = 0;

    public void RerollScreen()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
        // verticalLayoutGroup.enabled = false;
        // verticalLayoutGroup.enabled = true;
        Debug.Log("리롤"); 
        //changeCount = 1;
        //before_pos_h = parentLayout.sizeDelta.y;
    }
    
    //화면 새로고침
    
}
