using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "RecruitmentManagerSO", menuName = "ScriptableObject/Manager/RecruitmentManagerSO")]
public class RecruitmentManagerSO : ScriptableObject
{
    //채용 리스트
    List<Recruitment> recruitments;
    int id = 0; //recruitment id

    [SerializeField] List<EmployeeSO> employeeSOs; //employee types
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private EmployeeNameSO employeeNameSO;

    [Header("Manager")]
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [Header("Listening to Events")]
    [SerializeField] private BoolEventChannelSO _isChangedEvent;
    
    [Space]

    [Header("Broadcasting on FirebaseEvents")]
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    //채용 정보 [버튼을 누르면 함수를 호출해서 tmp처럼 대신 넣는 느낌]
    private int employeeTypeIndex = 0; //0,1,2,3
    private int level = 0; //0,1,2
    private int period = 1; //1 3 7
    private int cost; //코스트 => 게임 오브젝트도 가져와서 설정해야 할 거 같은데


    void OnEnable()
    {
    }

    void OnDisable()
    {
    }
    
    public void Init()
    {
        recruitments = new List<Recruitment>();
        
    }
    
    #region PROPERTY

    public void SetID()
    {
        id = 0;
        if (recruitments.Count != 0)
        {
            id = recruitments[recruitments.Count - 1].GetID() + 1;
        }
    }
    
    public void SetEmployeeType(int index)
    {
        employeeTypeIndex = index;
        /*
        switch (index)
        {
            case 0:
                employeeTypeIndex = (int)EmployeeType.PRODUCT_MANAGER;
                break;
            case 1:
                employeeTypeIndex = (int)EmployeeType.DEVELOPER;
                break;
            case 2:
                employeeTypeIndex = (int)EmployeeType.DESIGNER;
                break;
            case 3:
                employeeTypeIndex = (int)EmployeeType.QA;
                break;
        }
        */
    }
    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void Setperiod(int index)
    {
        switch (index)
        {
            case 0:
                period = 1;
                break;
            case 1:
                period = 3;
                break;
            case 2:
                period = 7;
                break;
        }
    }

    public void AddRecruitment()
    {
        Recruitment recruitment = new Recruitment();
        recruitment.Init(RemoveServerApplicant);

        //버튼 정보 가져오기 디버깅
        recruitment.SetEmployeeSO(employeeSOs[employeeTypeIndex]);
        recruitment.SetLevel(level);
        recruitment.SetDay(period);
        recruitment.SetID(id++);
        //SetCost()

        recruitments.Add(recruitment);
        AddServerRecruitmentIndex(Search_Recruitment_Index(recruitment.GetID()));

        //bool true
        if (_isChangedEvent == null)
        {
            Debug.LogError("This message should never appear");
            return;
        }
        _isChangedEvent.RaiseEvent(true);
    }

    //randomMax 값 만큼 랜덤 돌리기
    // ReSharper disable Unity.PerformanceAnalysis
    public void AddRandomApplicants(int randomWeight)
    {
        for (int i = 0; i < recruitments.Count; i++)
        {
            int value = Random.Range(0, randomWeight); //randomMax
            if (value == 0)
            {
                string name = employeeNameSO.GetRandomName();
                
                recruitments[i].AddApplicant(
                    CreateApplicant(
                        name,
                        recruitments[i].GetID(),
                        recruitments[i].GetEmployeeSO()
                    )
                );
            }
        }
        _isChangedEvent.RaiseEvent(true);
    }
    
    //지원자 넣는 함수 => 나중에 Employee 매개변수 받고 생성할 예정
    public Employee CreateApplicant(string name, int id, EmployeeSO employeeSO)
    {
        Employee employee = new Employee(_employeeManagerSO, false);
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
        employee._EmployeeSO = employeeSO;
        //(int)(recruitment.GetEmployeeSO().GetEmployeeType()

        //서버에 들어가버려잇
        SetServerApplicant(id, employee);

        return employee;
    }

    /// <summary>
    /// 서버 포함 제거
    /// </summary>
    /// <param name="id">recruitment_id</param>
    public void RemoveRecruitment(int id)
    {
        int index = Search_Recruitment_Index(id);
        recruitments.RemoveAt(index);
        RemoveServerRecruitment(id);        //서버 연동
        _isChangedEvent.RaiseEvent(true);
    }

    //server + isChanged
    public void RemoveApplicant(int recruitment_id, int applicant_id)
    {
        recruitments[recruitment_id].RemoveApplicant(applicant_id);
        
        RemoveServerApplicant(recruitment_id, applicant_id);
        _isChangedEvent.RaiseEvent(true);
        
    }
    
    public EmployeeSO GetEmployeeSO(int index)
    {
        return employeeSOs[index];
    }
    
    //recruitments

    public List<Recruitment> GetRecruitments()
    {
        return recruitments;
    }
    
    public Recruitment GetRecruitment(int id)
    {
        return recruitments[id];
    }
    

    #endregion

    #region SERVER
    
    //recruitment
    public void AddServerRecruitmentIndex(int index) //recruit 인덱스만 서버 동기화 => Firestore 배열 Add 기능만 있음
    {
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "recruitments." + index.ToString(),
            recruitments[index].RecruitmentToJSON()
        );
        //recruitments.index => 
        //FieldValue.ArrayUnion(recruitments[index].RecruitmentToJSON()) //기존에 있는 배열에서 추가한 느낌

    }

    public void RemoveServerRecruitment(int id)
    {
        _deleteFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "recruitments." + id.ToString()
        );
    }

    //applicant
    public void RemoveServerApplicant(int recruitment_id, int applicant_id)
    {
        //recruitment의 id, applicant_id는 지원자의 id
        string id = recruitment_id.ToString();
        _deleteFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "recruitments." + id + ".applicants." + applicant_id.ToString()
        );
    }
    public void SetServerApplicant(int recruitment_id, Employee applicant)
    {
        _sendFirebaseEventChannelSO.RaiseEvent("GamePlayUser",
            GameManager.instance.Nickname,
            "recruitments." + recruitment_id.ToString() + ".applicants." + applicant.ID,
            applicant.EmployeeToJSON()
        );
    }

    //init
    public void JSONToRecruitments(Dictionary<string, object> serverRecruitments) //server 예정
    {
        if (this.recruitments == null)
            this.recruitments = new List<Recruitment>();

        // null 처리
        if (serverRecruitments != null)
        {
            //map형태의 recruitments를 list로 변환
            foreach (KeyValuePair<string, object> serverRecruitment in serverRecruitments)
            {
                Recruitment recruitment = new Recruitment();
                recruitment.JSONToRecruitment(serverRecruitment, this, _employeeManagerSO);
                this.recruitments.Add(recruitment);
            }
        }
        SetID();
    }

    #endregion

    #region BINARY_SEARCH

    //Recruitment 이진탐색
    public int Search_Recruitment_Index(int id)
    {
        int index = Binary_Search_Recruitment_Index(0, recruitments.Count - 1, id);
        if (recruitments[index].GetID() == id)
        {
            return index;
        }
        return -1;
    }

    private int Binary_Search_Recruitment_Index(int start, int end, int id)
    {
        if (start > end)
        {
            return start;
        }
        int mid = (start + end) / 2;
        if (id > recruitments[mid].GetID())
        {
            return Binary_Search_Recruitment_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Recruitment_Index(start, mid - 1, id);
        }
    }

    #endregion
    

}
