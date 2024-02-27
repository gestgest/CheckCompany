using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    public void SetMoneyText(int value)
    {
        moneyText.text = value.ToString();
    }
}
