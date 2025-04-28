using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//나중에 dropbox 인터페이스 만들 예정 [소공] => 채용 공고 드롭박스 클래스
public class RecruitmentDropbox : MonoBehaviour
{
    [SerializeField] private RecruitmentsSO recruitmentsSO;

    TMP_Dropdown dropdown;
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
        recruitmentsSO.SetEmployeeType(change_dropdown.value);
    }

}
