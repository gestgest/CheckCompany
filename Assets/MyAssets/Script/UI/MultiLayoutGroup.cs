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
    bool isInit = false;

    //private void Awake()
    //{
    //    //tmi. Awake는 비활성화 되어도 실행되는 건가?
    //    //ChatGPT 답변 : Awake는 생성되기 전에 호출하는 느낌이라 parent 지정을 아직 못한다.
    //    //RecruitmentElement에서는 나중에 부모를 설정했다.
    //    //
    //}
    //생성될때 
    public void Init()
    {
        _parentMultiLayout = transform.parent.GetComponent<MultiLayoutGroup>();
        size = transform.GetComponent<RectTransform>();
        layoutGroup = transform.GetComponent<VerticalLayoutGroup>();

        isInit = true;
        if (_parentMultiLayout == null)
        {
            return;
        }

        if(!_parentMultiLayout.GetIsInit())
            _parentMultiLayout.Init();


        //if (size == null)
        //{
        //    Debug.Log(gameObject.name + " RectTransform 없다네");
        //}
        //else
        //{
        //    Debug.Log(gameObject.name + " has RectTransform");
        //}

        //if (layoutGroup == null)
        //{
        //    Debug.Log(gameObject.name + " VerticalLayoutGroup 없다네");
        //}
        //else
        //{
        //    Debug.Log(gameObject.name + " has VerticalLayoutGroup");
        //}
    }


    void Start()
    {

        //자식들 크기 저장
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
            //if (layoutGroup == null)
            //{
            //    Debug.Log(gameObject.name + " ?");
            //    layoutGroup = transform.GetComponent<VerticalLayoutGroup>();
            //}
            Debug.Log(gameObject.name + " : 엄준식");

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
    public bool GetIsInit()
    {
        return isInit;
    }
}
