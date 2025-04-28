using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "RecruitmentsSO", menuName = "ScriptableObject/Model/RecruitmentsSO")]
public class RecruitmentsSO : ScriptableObject
{
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects; //RecruitmentElement
    int id = 0; //생성하려는 recruitment id

    [SerializeField] List<EmployeeSO> employeeSOs; //employee 특징
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private EmployeeNameSO employeeNameSO;

    //init로 넣어야 할 정보
    private GameObject view; //parent
    private TextMeshProUGUI costText;


    //채용 정보 [버튼을 누르면 함수를 호출해서 tmp처럼 대신 넣는 느낌]
    private int employeeTypeIndex = 0; //0,1,2,3
    private int level = 0; //0,1,2
    private int period = 1; //1 3 7
    private int cost; //코스트 => 게임 오브젝트도 가져와서 설정해야 할 거 같은데



    private void Start()
    {
        recruitments = new List<Recruitment>();
        recruitmentObjects = new List<GameObject>();

        //서버에서 recruitments 가져오는 함수() => 이미 init로 함

        //dictionary => list로 변환하는 함수

        //InitRecruitments(); //recruitments => 오브젝트
        //ShowRecruitments();
    }

    public void Init(GameObject view, TextMeshProUGUI costText)
    {
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

        recruitmentObjects.Add(recruitmentObject);
        //recruitmentObject.transform.SetParent(view.transform);

        recruitmentContent.SetApplicant(); //그리기
    }

    //추가하는 함수
    public void AddRecruitment()
    {
        Recruitment recruitment = new Recruitment();
        recruitment.Init(this);

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
    
    public void RecruitmentsFromJSON(Dictionary<string, object> serverRecruitments) //server 예정
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
                recruitment.RecruitmentFromJSON(serverRecruitment, this);
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

    public void AddServerRecruitmentIndex(int index) //recruit 인덱스만 서버 동기화 => Firestore 배열 Add 기능만 있음
    {
        //Dictionary<string, object> data = new Dictionary<string, object>
        //{
        //    { index.ToString(), recruitments[index].RecruitmentToJSON() }
        //};

        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname,
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
