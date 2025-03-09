using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// <summary> Layout에만 적으면 됨</summary>
public class MultiLayoutGroup : MonoBehaviour
{
    RectTransform size; //자기 자신 size
    private float onHeight = 0;
    private bool isOn = false;
    
    private VerticalLayoutGroup layoutGroup;
    private MultiLayoutGroup _parentMultiLayout = null;


    void Start()
    {
        _parentMultiLayout = transform.parent.GetComponent<MultiLayoutGroup>();
        size = transform.GetComponent<RectTransform>();
        layoutGroup = GetComponent<VerticalLayoutGroup>();

        //자식들 크기 비교
        for (int i = 0; i < transform.childCount; i++)
        {
            onHeight += transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
        }
    }

    public void SwitchingScreen()
    {
        if (isOn)
        {
            isOn = false;
            AddHeight(-onHeight);
        }
        else
        {
            isOn = true;
            AddHeight(+onHeight);
        }
        RerollScreen();
    }
    public void RerollScreen()
    {
        if (_parentMultiLayout == null)
        {
            Canvas.ForceUpdateCanvases();
            
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
            return;
        }
        //LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
        
        _parentMultiLayout.RerollScreen();
        
        
        //before_pos_h = parentLayout.sizeDelta.y;
    }
    
    public void AddHeight(float height)
    {
        SetHeight(size.sizeDelta.y + height);
    }
    public void SetHeight(float height)
    {
        size.sizeDelta = new Vector2(size.sizeDelta.x, height);
        if (_parentMultiLayout != null)
        {
            _parentMultiLayout.AddHeight(height);
            RerollScreen();
        }
    }

    public void AddOnHeight(float height)
    {
        onHeight += height;
        if (_parentMultiLayout != null)
        {
            _parentMultiLayout.AddOnHeight(height);
        }
        else
        {
            RerollScreen();
        }
    }
        
}
