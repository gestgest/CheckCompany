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
    [SerializeField] private TextMeshProUGUI applicantNumber_Text;
    [SerializeField] private GameObject applicantsPanel; //지원자 리스트


    [SerializeField] private GameObject applicant_Prefab;
    private Transform layout_parent;
    private VerticalLayoutGroup parent_VLG;


    //지원자 정보 리스트
    private List<IEmployee> applicants; //정렬된 상태어야 하는데
    private List<GameObject> applicant_objects;

    public int ID { get; set; } //채용 구분 ID
    private EmployeeSO employeeSO;

    private void Start()
    {
        applicants = new List<IEmployee>();
        applicant_objects = new List<GameObject>();
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
            employee.ID = GameManager.instance.Employee_count;
            GameManager.instance.Employee_count = employee.ID + 1;
            employee.Name = "문재현";
            employee.Age = 19;
            employee.CareerPeriod = 12; //1 year
            employee.Salary = 100; //월 100만원
            employee._EmployeeSO = RecruitmentController.instance.GetRecruitmentEmployeeSO((int)(employeeSO.GetEmployeeType())); //recruitment에 걸맞게

            //서버에 들어가버려잇

            applicants.Add(employee);
            SelectionApplicantSort();
            SetApplicantsNumber(applicants.Count); //지원자 수
            CreateEmployeeObject(employee);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("RecruitmentElement : T 버튼 누름");
            RerollPanel();
        }
    }

    public void SetRecruitment(EmployeeSO employeeSO, int day, int size, int id)
    {
        this.employeeSO = employeeSO;
        SetIcon(employeeSO.GetIcon());
        SetDDay(day);
        SetApplicantsNumber(size);
        ID = id;
    }

    private void CreateEmployeeObject(IEmployee employee)
    {
        GameObject tmp = Instantiate(applicant_Prefab);

        ApplicantElement applicantElement = tmp.GetComponent<ApplicantElement>();
        applicantElement.SetValue(employee, ID);

        tmp.transform.SetParent(applicantsPanel.transform);
        applicant_objects.Add(tmp);
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


    public IEmployee GetApplicant(int id)
    {
        int index = Search_Employee_Index(id);
        if (index == -1)
            return null;
        return applicants[index];

    }

    #endregion 

    //이진탐색
    public int Search_Employee_Index(int id)
    {
        return Binary_Search_Employee_Index(0, applicants.Count - 1, id);
    }

    private int Binary_Search_Employee_Index(int start, int end, int id)
    {
        if (start > end) return -1;
        int mid = (start + end) / 2;

        //아이디가 같다
        if (applicants[mid].ID == id)
        {
            return mid;
        }
        else if (id > applicants[mid].ID)
        {
            return Binary_Search_Employee_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Employee_Index(start, mid - 1, id);
        }

    }

    public void RemoveApplicant(int id)
    {
        int index = Search_Employee_Index(id);

        applicants.RemoveAt(index);
        Destroy(applicant_objects[index]); //Pool링?
        applicant_objects.RemoveAt(index);
        SetApplicantsNumber(applicants.Count);

    }

    public void SwitchPanel()
    {
        applicantsPanel.SetActive(!(applicantsPanel.activeSelf));
        RerollPanel();
    }


    //dropButton, 배치관리자가 제대로 안되는거 방지
    private void RerollPanel()
    {
        parent_VLG.childControlHeight = false;
        parent_VLG.childControlHeight = true;
    }

    private void SelectionApplicantSort()
    {
        //O(n^2) => 나중에 다른 정렬로 바꾸지 않을까
        for(int i = 0; i < applicants.Count; i++)
        {
            for (int j = i + 1; j < applicants.Count; j++)
            {
                if (applicants[i].ID > applicants[j].ID)
                {
                    IEmployee tmp = applicants[i];
                    applicants[i] = applicants[j];
                    applicants[j] = tmp;

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

}
