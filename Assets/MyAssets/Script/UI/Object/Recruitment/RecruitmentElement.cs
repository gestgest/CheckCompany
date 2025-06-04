using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
    private RectTransform layout_parent;
    
    [SerializeField]private MultiLayoutGroup _multiLayoutGroup; //applicantsPanel

    //Controller
    [Header("Controller")]
    [SerializeField] private RecruitmentManagerSO _recruitmentControllerSO;
    [SerializeField] private EmployeeManagerSO _employeeControllerSO;

    //지원자 정보 리스트
    private Recruitment recruitment;
    private List<GameObject> applicant_objects;

    private static int HEIGHT = 100;
    private void Start()
    {
        //아니 이게 왜 필요하지
        //recruitment.Init();

        //Init();

    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("RecruitmentElement : T 버튼 누름");
            _multiLayoutGroup.RerollScreen();
        }
    }

    public void Init()
    {
        layout_parent = transform.parent.GetComponent<RectTransform>(); //부모 가져오기
        _multiLayoutGroup.Init();
        //_multiLayoutGroup.AddHeight(HEIGHT);
        
        //부모 디폴트 높이 추가
        Vector2 v = layout_parent.sizeDelta;
        v.y += HEIGHT;
        layout_parent.sizeDelta = v;
        
        //_multiLayoutGroup.AddOnHeight(-HEIGHT);
        //레이아웃 처음 만들때 자기 자신은 제외.
        //자기 자신 더하는 함수는 LayoutGroup의 Start에서
        
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

        //RerollScreen();
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

    public void SetRecruitment(Recruitment recruitment)
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
        
        _multiLayoutGroup.AddOnHeight(HEIGHT);
    }

    public void RemoveRecruitment()
    {
        _recruitmentControllerSO.RemoveRecruitment(recruitment.GetID());
        _multiLayoutGroup.AddOnHeight(-HEIGHT);
        _multiLayoutGroup.RerollScreen();
        Destroy(gameObject);
        //이 오브젝트 제거
    }


    //지원자 넣는 함수 => 나중에 Employee 매개변수 받고 생성할 예정
    public void AddApplicant(string name)
    {
        Employee employee = new Employee(_employeeControllerSO, false);
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
        employee.Salary = 1000000; //월 100만원
        employee._EmployeeSO = _recruitmentControllerSO.GetRecruitmentEmployeeSO(
            _recruitmentControllerSO.Search_Recruitment_Index(recruitment.GetID())
        );
        //(int)(recruitment.GetEmployeeSO().GetEmployeeType()

        //서버에 들어가버려잇
        _recruitmentControllerSO.SetServerApplicant(recruitment.GetID(), employee);

        recruitment.AddApplicant(employee);
        SetApplicant(employee);
    }



    #region PROPERTY
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
        SwitchingScreen(applicantsPanel.activeSelf);
    }

    private void SwitchingScreen(bool isShow)
    {
        _multiLayoutGroup.SwitchingScreen(isShow);
    }


    #region binary_search
    //이진탐색

    public void RemoveApplicant(int applicant_id)
    {
        int index = recruitment.Search_Employee_Index(applicant_id);

        //_recruitmentControllerSO.RemoveServerApplicant(recruitment.GetID(), applicant_id);
        Destroy(applicant_objects[index]); //Pool링?
        applicant_objects.RemoveAt(index);
        recruitment.RemoveApplicant(applicant_id, index);
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
