using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//채용 칸 스크립트
public class RecruitmentElement : MonoBehaviour
{
    //모델
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI dDayText;
    [SerializeField] private TextMeshProUGUI RecruitmentNumber_Text;
    [SerializeField] private GameObject applicantPanel;
    

    [SerializeField] private GameObject applicant_Prefab;
    private Transform layout_parent;
    private VerticalLayoutGroup parent_VLG;


    //지원자 정보 리스트
    private List<IEmployee> employees;
    private List<GameObject> employee_GameObjects;

    public int ID { get; set; } //채용 구분 ID

    private void Start()
    {
        employees = new List<IEmployee>();
        employee_GameObjects = new List<GameObject>();
        layout_parent = transform.parent; //부모 가져오기
        parent_VLG = layout_parent.GetComponent<VerticalLayoutGroup>(); //부모의 layout 가져오기
        //icon.sprite

    }

    private void Update()
    {
        //키보드 R 누르면
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("RecruitmentElement : R 버튼 누름");
            IEmployee employee = new Development();
            employee.ID = GameManager.instance.Executive_count;
            GameManager.instance.Executive_count = employee.ID + 1;
            employee.Name = "문재현";
            employee.Age = 19;
            employee.CareerPeriod = 12; //1 year
            employee.Salary = 100; //월 100만원

            employees.Add(employee);
            SetRecruitmentNumber(employees.Count);
            CreateEmployeeObject(employee);
        }
    }

    public void SetRecruitment(Sprite sprite, int day, int size, int id)
    {
        SetIcon(sprite);
        SetDDay(day);
        SetRecruitmentNumber(size);
        ID = id;
    }

    private void CreateEmployeeObject(IEmployee employee)
    {
        GameObject tmp = Instantiate(applicant_Prefab);

        ApplicantElement applicantElement = tmp.GetComponent<ApplicantElement>();
        applicantElement.SetValue(employee);

        tmp.transform.SetParent(applicantPanel.transform);
        employee_GameObjects.Add(tmp);
    }



    private void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    private void SetDDay(int day)
    {
        dDayText.text = day.ToString();
    }
    private void SetRecruitmentNumber(int size)
    {
        RecruitmentNumber_Text.text = size.ToString() + "명";
    }

    //dropButton
    public void SwitchingPanel()
    {
        parent_VLG.childControlHeight = false;
        applicantPanel.SetActive(!(applicantPanel.activeSelf));
        parent_VLG.childControlHeight = true;
    }


}
