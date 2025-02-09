using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI DateText;

    public void SetMoneyText(long value)
    {
        moneyText.text = value.ToString();
    }

    public void SetDateText(Date value)
    {
        DateText.text = value.ToString();
    }
}
