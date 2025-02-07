using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//채용 칸 스크립트
public class RecruitmentElement : MonoBehaviour
{
    //모델
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI dDayText;
    [SerializeField] private TextMeshProUGUI applicantNumber_Text;
    [SerializeField] private GameObject applicantsPanel; //지원자 리스트


    [SerializeField] private GameObject applicant_Prefab;
    [SerializeField] private Transform layout_parent;
    private VerticalLayoutGroup parent_VLG;


    //지원자 정보 리스트
    private Recruitment recruitment;
    private List<GameObject> applicant_objects;


    private void Start()
    {
        recruitment.Init();
        Init();
        layout_parent = transform.parent; //부모 가져오기
        parent_VLG = layout_parent.GetComponent<VerticalLayoutGroup>(); //부모의 layout 가져오기

        //RerollPanel();
        //icon.sprite
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("RecruitmentElement : T 버튼 누름");
            RerollPanel();
        }
    }

    public void Init()
    {
        if(applicant_objects == null)
            applicant_objects = new List<GameObject>();

    }

    /// <summary>
    /// 한번에 그리는 함수
    /// </summary>
    public void SetApplicant()
    {
        int applicant_size = recruitment.GetApplicantCount();
        SetApplicantsNumber(applicant_size); //지원자 수


        //오브젝트 생성
        for (int i = 0; i < applicant_size; i++)
        {
            CreateEmployeeObject(recruitment.GetApplicant(i));
        }
        SelectionApplicantSort();
        //RerollPanel();
    }

    /// <summary>
    /// 지원자로 오브젝트 그리는 함수
    /// </summary>
    /// <param name="employee"></param>
    public void SetApplicant(Employee employee)
    {
        SetApplicantsNumber(recruitment.GetApplicantCount()); //지원자 수
        CreateEmployeeObject(employee);
        SelectionApplicantSort();

        //RerollPanel();
    }

    public void SetRecruitment(Recruitment recruitment) //나중에 매개변수를 Recruitment으로 해라 ㅇㅇ
    {
        this.recruitment = recruitment;
        SetIcon(recruitment.GetEmployeeSO().GetIcon());
        SetDDay(recruitment.GetDay());
        SetApplicantsNumber(recruitment.GetApplicantCount());
    }

    private void CreateEmployeeObject(Employee employee)
    {
        GameObject tmp = Instantiate(applicant_Prefab);

        ApplicantElement applicantElement = tmp.GetComponent<ApplicantElement>();
        applicantElement.SetValue(employee, recruitment.GetID());

        tmp.transform.SetParent(applicantsPanel.transform);
        applicant_objects.Add(tmp);
    }

    public void RemoveRecruitment()
    {
        RecruitmentController.instance.RemoveRecruitment(recruitment.GetID());
        Destroy(gameObject);
        //이 오브젝트 제거
    }
    
    private void SetServerApplicant(Employee applicant)
    {
       
        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname,
            "recruitments." + recruitment.GetID().ToString() + ".applicants." + applicant.ID,
            applicant.SetEmployeeToJSON()
        );
    }


    #region property

    private void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    private void SetDDay(int day)
    {
        dDayText.text = day.ToString();
    }
    private void SetApplicantsNumber(int size)
    {
        applicantNumber_Text.text = size.ToString() + "명";
    }

    //지원자 넣는 함수 => 나중에 Employee 매개변수 받고 생성할 예정
    public void AddApplicant(string name)
    {
        Employee employee = new Employee();
        employee.ID = GameManager.instance.Employee_count;
        GameManager.instance.Employee_count = employee.ID + 1;
        employee.Name = name;
        //employee.IsEmployee = false;
        employee.Age = 19;
        employee.Max_Stamina = 100; //이렇게 해야지 Max값으로 Stamina값 비교 가능
        employee.SetStamina(100, false);
        employee.Max_Mental = 100;
        employee.Mental = 100;
        employee.CareerPeriod = 12; //1 year
        employee.Salary = 100; //월 100만원
        employee._EmployeeSO = RecruitmentController.instance.GetRecruitmentEmployeeSO(
            RecruitmentController.instance.Search_Recruitment_Index(recruitment.GetID())
        );
        //(int)(recruitment.GetEmployeeSO().GetEmployeeType()

        //서버에 들어가버려잇
        SetServerApplicant(employee);

        recruitment.AddApplicant(employee);
        SetApplicant(employee);
    }

    public Employee GetApplicant(int id)
    {
        int index = recruitment.Search_Employee_Index(id);
        if (index == -1)
            return null;
        return recruitment.GetApplicant(index);
    }

    #endregion

    public void SwitchPanel()
    {
        applicantsPanel.SetActive(!(applicantsPanel.activeSelf));
        RerollPanel();
    }


    //dropButton, 배치관리자가 제대로 안되는거 방지
    private void RerollPanel()
    {
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parent_VLG.transform);
        parent_VLG.childControlHeight = false;
        parent_VLG.childControlHeight = true;
        Debug.Log("배치관리자 리롤"); 
    }


    #region binary_search
    //이진탐색

    public void RemoveApplicant(int applicant_id)
    {
        int index = recruitment.Search_Employee_Index(applicant_id);

        recruitment.RemoveServerApplicant(applicant_id);
        Destroy(applicant_objects[index]); //Pool링?
        applicant_objects.RemoveAt(index);
        recruitment.RemoveApplicant(index);
        SetApplicantsNumber(recruitment.GetApplicantCount());

    }

    private void SelectionApplicantSort()
    {
        //O(n^2) => 나중에 다른 정렬로 바꾸지 않을까
        for(int i = 0; i < recruitment.GetApplicantCount(); i++)
        {
            for (int j = i + 1; j < recruitment.GetApplicantCount(); j++)
            {
                if (recruitment.GetApplicant(i).ID > recruitment.GetApplicant(j).ID)
                {
                    recruitment.SwitchApplicant(i, j);

                    Transform a = applicant_objects[i].transform;
                    int a_index = a.GetSiblingIndex();
                    Transform b = applicant_objects[j].transform;
                    int b_index = b.GetSiblingIndex();

                    //Debug.Log("a : " +a_index);
                    //Debug.Log("b : " +b_index);
                    b.SetSiblingIndex(a_index);
                    a.SetSiblingIndex(b_index);

                    GameObject tmp_object = applicant_objects[i];
                    applicant_objects[i] = applicant_objects[j];
                    applicant_objects[j] = tmp_object;
                }
            }
        }
    }
    #endregion

}
