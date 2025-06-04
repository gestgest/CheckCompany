using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "RecruitmentManagerSO", menuName = "ScriptableObject/Controller/RecruitmentManagerSO")]
public class RecruitmentManagerSO : ScriptableObject
{
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects; //RecruitmentElement
    int id = 0; //생성하려는 recruitment id

    [SerializeField] List<EmployeeSO> employeeSOs; //employee 특징
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private EmployeeNameSO employeeNameSO;

    [Header("Manager")]
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [Space]
    [Header("Listening to Events")]
    [SerializeField] private GameObjectEventChannelSO _addRecruitmentObject;

    [Header("Broadcasting on FirebaseEvents")]
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    
    
    //init로 넣어야 할 정보
    private GameObject view; //parent
    private TextMeshProUGUI costText;


    //채용 정보 [버튼을 누르면 함수를 호출해서 tmp처럼 대신 넣는 느낌]
    private int employeeTypeIndex = 0; //0,1,2,3
    private int level = 0; //0,1,2
    private int period = 1; //1 3 7
    private int cost; //코스트 => 게임 오브젝트도 가져와서 설정해야 할 거 같은데


    void OnEnable()
    {
        _addRecruitmentObject._onEventRaised += AddRecruitmentObject;
    }

    void OnDisable()
    {
        _addRecruitmentObject._onEventRaised -= AddRecruitmentObject;
    }
    
    public void Init(GameObject view, TextMeshProUGUI costText)
    {
        recruitments = new List<Recruitment>();
        recruitmentObjects = new List<GameObject>();
        
        this.view = view;
        this.costText = costText;
    }

    //초기 채용공고 리스트 보여주는 함수
    public void ShowRecruitments()
    {
        SetID();

        for (int i = 0; i < recruitments.Count; i++)
        {
            Recruitment r = recruitments[i];
            CreateRecruitmentObject(r);
        }
    }

    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitmentObject(Recruitment r)
    {
        GameObject recruitmentObject = GameObject.Instantiate(recruitmentPrefab, view.transform);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i)
        recruitmentContent.Init(); //여기에 multiLayoutGroup height값을 추가
        recruitmentContent.SetRecruitment(r);


        recruitmentContent.SetApplicant(); //그리기
    }

    private void AddRecruitmentObject(GameObject recruitmentObject)
    {
        recruitmentObjects.Add(recruitmentObject);
    }

    //추가하는 함수
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

        Debug.Log(recruitment.GetID());
        recruitments.Add(recruitment);
        AddServerRecruitmentIndex(Search_Recruitment_Index(recruitment.GetID()));

        CreateRecruitmentObject(recruitment);
    }

    #region Property

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

    private void SetCost(int cost)
    {
        this.cost = cost;
        costText.text = cost.ToString();
    }

    //index는 recruitment의 index임
    public EmployeeSO GetRecruitmentEmployeeSO(int index)
    {
        return recruitments[index].GetEmployeeSO();
    }

    //randomMax 값 만큼 랜덤 돌리기
    // ReSharper disable Unity.PerformanceAnalysis
    public void AddApplicants(int randomWeight)
    {
        for (int i = 0; i < recruitments.Count; i++)
        {
            int value = Random.Range(0, randomWeight); //randomMax
            if (value == 0)
            {
                string name = employeeNameSO.GetRandomName();

                recruitmentObjects[i].GetComponent<RecruitmentElement>().AddApplicant(name);
            }
        }
    }

    /// <summary>
    /// 서버 포함 제거
    /// </summary>
    /// <param name="id">recruitment_id</param>
    public void RemoveRecruitment(int id)
    {
        int index = Search_Recruitment_Index(id);
        recruitments.RemoveAt(index);
        recruitmentObjects.RemoveAt(index);

        //서버 연동
        RemoveServerRecruitment(id);
    }

    public EmployeeSO GetEmployeeSO(int index)
    {
        return employeeSOs[index];
    }

    public Recruitment GetRecruitment(int id) // property
    {
        return recruitments[id];
    }
    
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

        ShowRecruitments();
    }


    public GameObject GetRecruitmentObject(int index)
    {
        return recruitmentObjects[index];
    }
    public GameObject GetLastRecruitmentObject()
    {
        return recruitmentObjects[recruitmentObjects.Count - 1];
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


    #endregion

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

}
