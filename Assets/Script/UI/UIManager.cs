using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject [] windows;

    private void Start()
    {
        //일단 모든 윈도우 창 비활성화
        // ++ 그냥 윈도우 부모를 가져와서 하위 윈도우를 가져가는게 어떨까? => 윈도우 창이 아무리 추가되어도 설정 안해도 됨
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].gameObject.SetActive(false);
        }
    }
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
