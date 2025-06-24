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
    
    [SerializeField] private MultiLayoutGroup _multiLayoutGroup; //applicantsPanel

    //Controller
    [Header("Manager")]
    [SerializeField] private RecruitmentManagerSO _recruitmentControllerSO;
    [SerializeField] private EmployeeManagerSO _employeeControllerSO;

    //지원자 정보 리스트
    private Recruitment recruitment;
    private List<GameObject> applicant_objects;

    private static int HEIGHT = 100;
    //change event => RecruitPanel's persistent
    
    private void OnEnable()
    {
        //SetUI();
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(_multiLayoutGroup.GetOnHeight());
            _multiLayoutGroup.RerollScreen();
        }
    }

    //init
    public void SetEmployee(Recruitment recruitment)
    {
        layout_parent = transform.parent.GetComponent<RectTransform>(); //부모 Layout 가져오기
        _multiLayoutGroup.Init();
        
        //_multiLayoutGroup.AddHeight(HEIGHT);
        
        //부모 디폴트 높이 추가
        // Vector2 v = layout_parent.sizeDelta;
        // v.y += HEIGHT;
        // layout_parent.sizeDelta = v;
        
        //_multiLayoutGroup.AddOnHeight(-HEIGHT);
        //레이아웃 처음 만들때 자기 자신은 제외.
        //자기 자신 더하는 함수는 LayoutGroup의 Start에서
        
        if(applicant_objects == null)
            applicant_objects = new List<GameObject>();
        
        this.recruitment = recruitment;

        SetUI();
    }

    private void SetUI()
    {
        SetIcon(recruitment.GetEmployeeSO().GetIcon());
        SetDDay(recruitment.GetDay());
        SetApplicantsNumber(recruitment.GetApplicantCount());
        
        //RemoveAllApplicantObjects(); => 정말 상관없음, 어차피 RecruitPanel에서 다 새로로 만들어서
        AllCreateApplicantObjects();
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
    
    /// <summary>
    /// 한번에 그리는 함수
    /// </summary>
    public void AllCreateApplicantObjects()
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


    private void CreateEmployeeObject(Employee employee)
    {
        GameObject tmp = Instantiate(applicant_Prefab, applicantsPanel.transform);

        ApplicantElement applicantElement = tmp.GetComponent<ApplicantElement>();
        applicantElement.SetValue(employee, recruitment.GetID());

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

    // private void RemoveAllApplicantObjects()
    // {
    //     //만약 지원자 리스트들이 보이게 하고 나간다면 => 부모 오브젝트는 크기만 커지고 onHeight는 그대로
    //     if(applicantsPanel.activeSelf)
    //         _multiLayoutGroup.SwitchingScreen(false); //비활성화 => _multiLayoutGroup.AddHeight(-_multiLayoutGroup.GetOnHeight());
    //      
    //     _multiLayoutGroup.AddOnHeight(-_multiLayoutGroup.GetOnHeight()); //부모 오브젝트까지 초기화
    //     
    //     for (int i = 0; i < applicant_objects.Count; i++)
    //     {
    //         Destroy(applicant_objects[i]);
    //     }
    //     applicant_objects.Clear();
    // }


    public void SwitchPanel()
    {
        applicantsPanel.SetActive(!(applicantsPanel.activeSelf));
        SwitchingScreen(applicantsPanel.activeSelf);
    }

    private void SwitchingScreen(bool isShow)
    {
        _multiLayoutGroup.SwitchingScreen(isShow);
    }

    public void InitMultiLayoutGroup()
    {
        _multiLayoutGroup.SwitchingScreen(false);
        _multiLayoutGroup.AddOnHeight(-_multiLayoutGroup.GetOnHeight()); //부모 오브젝트까지 초기화
    }

    #region binary_search
    //이진탐색

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
