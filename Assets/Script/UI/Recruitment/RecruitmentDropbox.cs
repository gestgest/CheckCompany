using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//나중에 dropbox 인터페이스 만들 예정 [소공]
public class RecruitmentDropbox : MonoBehaviour
{
    TMP_Dropdown dropdown;
    [SerializeField] private RecruitmentController recruitmentController;
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownChanged(dropdown);
        });
    }
    void DropdownChanged(TMP_Dropdown change_dropdown)
    {
        //대충 controller의 함수 조절
        recruitmentController.SetEmployeeType(change_dropdown.value);
    }

}
