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
    
    private MultiLayoutGroup _parentMultiLayout = null;
    private VerticalLayoutGroup layoutGroup;

    private void Awake()
    {
        _parentMultiLayout = transform.parent.GetComponent<MultiLayoutGroup>();
        size = transform.GetComponent<RectTransform>();
        layoutGroup = transform.GetComponent<VerticalLayoutGroup>();

        if (size == null)
        {
            Debug.Log(gameObject.name + " RectTransform 없다네");
        }
        else
        {
            Debug.Log(gameObject.name + " has RectTransform");
        }
        
        if (layoutGroup == null)
        {
            Debug.Log(gameObject.name + " VerticalLayoutGroup 없다네");
        }
        else
        {
            Debug.Log(gameObject.name + " has VerticalLayoutGroup");
        }
    }

    void Start()
    {
        //자식들 크기 비교
        for (int i = 0; i < transform.childCount; i++)
        {
            onHeight += transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
        }
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
            AddHeight(+onHeight);
        }
        else
        {
            AddHeight(-onHeight);
        }
        RerollScreen();
    }
    public void RerollScreen()
    {
        if (_parentMultiLayout == null)
        {
            Canvas.ForceUpdateCanvases();

            //LayoutRebuilder.ForceRebuildLayoutImmediate(size);
            // if (layoutGroup == null)
            // {
            //     Debug.Log(gameObject.name + " ?");
            //     layoutGroup = transform.GetComponent<VerticalLayoutGroup>();
            // }
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
            return;
        }
        
        _parentMultiLayout.RerollScreen();
        //before_pos_h = parentLayout.sizeDelta.y;
    }
    
    public void AddHeight(float height)
    {
        Debug.Log(gameObject.name + " : " + height);
        SetHeight(size.sizeDelta.y + height);
        
        if (_parentMultiLayout != null)
        {
            _parentMultiLayout.AddHeight(height);
            RerollScreen();
        }
    }
    public void SetHeight(float height)
    {
        size.sizeDelta = new Vector2(size.sizeDelta.x, height);
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
