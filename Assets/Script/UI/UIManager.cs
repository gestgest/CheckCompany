using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Transform window_parent;

    private void Start()
    {
        AllCloseWindow();
    }
    
    private void Update()
    {
        //build의 B를 누르면 Build 창 뜸
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchingWindow(1);
        }
    }

    public void SetMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }

    public void AllCloseWindow()
    {
        //일단 모든 윈도우 창 비활성화
        for(int i = 0;i < window_parent.childCount; i++)
        {
            window_parent.GetChild(i).gameObject.SetActive(false);
        }
    }

    //윈도우 창 스위칭
    public void SwitchingWindow(int index)
    {
        bool isActive = window_parent.GetChild(index).gameObject.activeSelf;
        window_parent.GetChild(index).gameObject.SetActive(!isActive);
    }
    
    //윈도우 창 스위칭
    public void ShowWindow(int index)
    {
        AllCloseWindow();
        window_parent.GetChild(index).gameObject.SetActive(true);
    }
}
