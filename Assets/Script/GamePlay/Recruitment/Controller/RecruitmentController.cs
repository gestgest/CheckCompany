using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Firestore;
using Random = UnityEngine.Random;

//채용 공고 컨트롤러
public class RecruitmentController : MonoBehaviour
{
    public static RecruitmentController instance;
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects; //RecruitmentElement
    int id = 0; //생성하려는 recruitment id
    
    [SerializeField] List<EmployeeSO> employeeSOs; 
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private EmployeeNameSO employeeNameSO;

    //채용 정보 [버튼을 누르면 함수를 호출해서 tmp처럼 대신 넣는 느낌]
    private int employeeTypeIndex = 0; //0,1,2,3
    private int level = 0; //0,1,2
    private int period = 1; //1 3 7
    private int cost; //코스트 => 게임 오브젝트도 가져와서 설정해야 할 거 같은데

    


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        recruitments = new List<Recruitment>();
        recruitmentObjects = new List<GameObject>();

        //서버에서 recruitments 가져오는 함수() => 이미 init로 함

        //dictionary => list로 변환하는 함수

        //InitRecruitments(); //recruitments => 오브젝트
        //ShowRecruitments();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("RecruitmentController : W 버튼 누름 : " + GetRecruitmentEmployeeSO(0).GetEmployeeType());
        }
    }

    //pool 안 만들거임
    //초기 채용공고 리스트 보여주는 함수
    public void InitRecruitments()
    {
        id = recruitments.Count;
        for(int i = 0; i < recruitments.Count; i++)
        {
            Recruitment r = recruitments[i];
            CreateRecruitmentObject(r);
        }
    }

    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitmentObject(Recruitment r)
    {
        GameObject recruitmentObject = Instantiate(recruitmentPrefab, Vector3.zero, Quaternion.identity);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i)
        recruitmentContent.Init();
        recruitmentContent.SetRecruitment(r);
        recruitmentContent.SetApplicant(); //그리기

        recruitmentObjects.Add(recruitmentObject);
        recruitmentObject.transform.SetParent(view.transform);
    }

    //추가하는 함수
    public void AddRecruitment()
    {
        Recruitment recruitment = new Recruitment();
        recruitment.Init();

        //버튼 정보 가져오기 디버깅
        recruitment.SetEmployeeSO(employeeSOs[employeeTypeIndex]);
        recruitment.SetLevel(level);
        recruitment.SetDay(period);
        recruitment.SetID(id++);
        //SetCost()
        
        recruitments.Add(recruitment);
        Add_server_recruitment_index(recruitment.GetID());

        CreateRecruitmentObject(recruitment);
    }

    #region Property
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

    //id는 recruitment의 id임
    public EmployeeSO GetRecruitmentEmployeeSO(int id)
    {
        return recruitments[id].GetEmployeeSO();
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

    public void RemoveRecruitment(int id)
    {
        int index = Search_Recruitment_Index(id);
        recruitments.RemoveAt(index);
        recruitmentObjects.RemoveAt(index);

        //서버 연동
        Remove_server_recruitment_index(id);
    }
    
    public EmployeeSO GetEmployeeSO(int index)
    {
        return employeeSOs[index];
    }

    public Recruitment GetRecruitment(int id) // property
    {
        return recruitments[id];
    }
    public void GetRecruitmentsFromServer(Dictionary<string, object> serverRecruitments) //server 예정
    {
        if(this.recruitments == null)
            this.recruitments = new List<Recruitment>();


        //map형태의 recruitments를 list로 변환
        foreach (KeyValuePair<string, object> serverRecruitment in serverRecruitments)
        {
            Recruitment recruitment = new Recruitment();
            recruitment.JSONToRecruitment(serverRecruitment);
            this.recruitments.Add(recruitment);
        }

        InitRecruitments();
    }


    public GameObject GetRecruitmentObject(int index) 
    {
        Debug.Log(index);
        return recruitmentObjects[index];
    }

    #endregion

    public void Add_server_recruitment_index(int index) //recruit 인덱스만 서버 동기화 => Firestore 배열 Add 기능만 있음
    {
        //Dictionary<string, object> data = new Dictionary<string, object>
        //{
        //    { index.ToString(), recruitments[index].RecruitmentToJSON() }
        //};

        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname ,
            "recruitments." + index.ToString(),
            recruitments[index].RecruitmentToJSON()
        );

        //recruitments.index => 
        //FieldValue.ArrayUnion(recruitments[index].RecruitmentToJSON()) //기존에 있는 배열에서 추가한 느낌

        //user/
    }

    public void Remove_server_recruitment_index(int id)
    {
        FireStoreManager.instance.DeleteFirestoreDataKey(
            "GamePlayUser",
            GameManager.instance.Nickname, 
            "recruitments." + id.ToString()
        );
    }
    
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
