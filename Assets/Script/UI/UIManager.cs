using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject [] windows;
    public void SetMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }

    //윈도우 창 스위칭
    public void SwitchingWindow(int index)
    {
        bool isActive = windows[index].gameObject.activeSelf;
        windows[index].gameObject.SetActive(!isActive);
    }
}
