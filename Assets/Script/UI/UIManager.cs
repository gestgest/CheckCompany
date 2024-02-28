using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Window [] windows;
    public void SetMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }

    public void SwitchingWindow(int index)
    {
        bool isActive = windows[index].gameObject.activeSelf;
        windows[index].gameObject.SetActive(!isActive);
    }
}
