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
    
    private VerticalLayoutGroup layoutGroup;
    private MultiLayoutGroup _parentMultiLayout = null;

    private void Awake()
    {
        _parentMultiLayout = transform.parent.GetComponent<MultiLayoutGroup>();
        size = transform.GetComponent<RectTransform>();
        layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    void Start()
    {
        //자식들 크기 비교
        for (int i = 0; i < transform.childCount; i++)
        {
            onHeight += transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
        }
        Debug.Log(gameObject.name + " : " + onHeight);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log(gameObject.name + " : " + onHeight);
        }

    }

    public void SwitchingScreen(bool isOn)
    {
        if (isOn)
        {
            isOn = true;
            AddHeight(+onHeight);
        }
        else
        {
            isOn = false;
            AddHeight(-onHeight);
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
        //Debug.Log(onHeight);
        if (_parentMultiLayout != null)
        {
            Debug.Log(height);
            _parentMultiLayout.AddOnHeight(height);
        }
        else //부모인 경우
        {
            RerollScreen();
        }
    }
        
}
