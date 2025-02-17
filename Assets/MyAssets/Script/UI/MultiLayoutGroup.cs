using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MultiLayoutGroup : MonoBehaviour
{
    //여러가지 gameobject 그룹들 => transform
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private RectTransform parentRectTransform;
    
    public void RerollScreen()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
        //Debug.Log("리롤"); 
        // verticalLayoutGroup.enabled = false;
        // verticalLayoutGroup.enabled = true;
        //changeCount = 1;
        //before_pos_h = parentLayout.sizeDelta.y;
    }
    
    //화면 새로고침
    
}
