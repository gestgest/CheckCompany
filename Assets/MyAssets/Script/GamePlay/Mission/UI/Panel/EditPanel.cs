using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EditPanel : Panel
{
    [SerializeField] private TMP_InputField title;
    
    //라디오버튼 그룹 두개
    //ㄴ 나중에 SetMission때 활용
    
    [SerializeField] private CustomRadioButtonGroup employeeTypeGroup;
    [SerializeField] private CustomRadioButtonGroup levelGroup;
    [SerializeField] private GameObject [] smallMissions;

    [SerializeField] private MultiLayoutGroup layoutGroup;
    
    //소미션
    private int smallMission_size;
    //나중에 직원도 넣을 예정
    
    void Awake()
    {
        layoutGroup = GetComponent<MultiLayoutGroup>();
    }
    
    //init
    public void SetMission(Todo_Mission todoMission)
    {
        //값 넣기
        title.text = todoMission.GetName();
        employeeTypeGroup.SetToggle((int)todoMission.GetMissionType());        
        levelGroup.SetToggle(todoMission.GetLevel());        
        
        
        smallMission_size = 7;
        for (int i = 1; i < smallMission_size; i++)
        {
            DeleteSmallMission();
        }
        List<string> _smallMissions = todoMission.GetSmallMissions();
        smallMission_size = _smallMissions.Count;
        
        for (int i = 1; i < smallMission_size; i++)
        {
            //오브젝트 생성
            AddSmallMission();
        }
    }
    
    public void DeleteSmallMission()
    {
        if (smallMission_size <= 1)
            return;
        smallMission_size--;
        smallMissions[smallMission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        smallMissions[smallMission_size].SetActive(false);
        
        layoutGroup.RerollScreen();
    }

    //
    public void AddSmallMission()
    {
        if (smallMission_size == Employee.MAX_SMALL_MISSION_SIZE)
            return;
        smallMissions[smallMission_size].SetActive(true);
        smallMission_size++;
        
        layoutGroup.RerollScreen();
    }
    
    //최종적으로 값을 서버에 수정
    public void EditMission()
    {
        
    }
    //최종적으로 서버에 있는 미션 삭제
    public void DeleteMission()
    {
        
    }
}
