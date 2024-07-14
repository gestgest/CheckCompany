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
        //일단 모든 윈도우 창 비활성화
        for(int i = 0;i < window_parent.childCount; i++)
        {
            window_parent.GetChild(i).gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchingWindow(1);
        }
    }

    public void SetMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }

    //윈도우 창 스위칭
    public void SwitchingWindow(int index)
    {
        bool isActive = window_parent.GetChild(index).gameObject.activeSelf;
        window_parent.GetChild(index).gameObject.SetActive(!isActive);
    }
}
