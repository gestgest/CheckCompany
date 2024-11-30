using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Firestore;

//채용 공고 컨트롤러
public class RecruitmentController : MonoBehaviour
{
    public static RecruitmentController instance;
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects;
    int id = 0; //생성하려는 id
    
    [SerializeField] List<EmployeeSO> employeeSOs; 
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent
    [SerializeField] private TextMeshProUGUI costText;

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

        //서버에서 recruitments 가져오는 함수()

        InitRecruitments(); //초기 설정
        //ShowRecruitments();
    }

    //pool 안 만들거임
    //초기 채용공고 리스트 보여주는 함수
    private void InitRecruitments()
    {
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
        recruitmentContent.SetRecruitment(employeeSOs[(int)(r.GetEmployeeSO().GetEmployeeType())], r.GetDay(), 0, r.GetID());
        recruitmentObjects.Add(recruitmentObject);
        recruitmentObject.transform.SetParent(view.transform);
    }

    //추가하는 함수
    public void AddRecruitment()
    {
        Recruitment recruitment = new Recruitment();

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

    public EmployeeSO GetEmployeeSO(int id)
    {
        return recruitments[id].GetEmployeeSO();
    }
    public Recruitment GetRecruitment(int id) //server 예정
    {
        return recruitments[id];
    }


    public GameObject GetRecruitmentObject(int index) 
    {
        return recruitmentObjects[index];
    }

    #endregion

    public void Add_server_recruitment_index(int index) //recruit 인덱스만 서버 동기화 => Firestore 배열 Add 기능만 있음
    {
        long
        Dictionary<int, object> data = new Dictionary<int, object>
        {
            { 10, 10}
            //{ index, recruitments[index].RecruitmentToJSON() }
        };
        //대충 가져와서
        //

        //new Dictionary<int, object> { }
        FireStoreManager.instance.SetFirestoreData("GamePlayUser", "milkan660", "recruitments", data);

        //FieldValue.ArrayUnion(recruitments[index].RecruitmentToJSON()) //기존에 있는 배열에서 추가한 느낌

        //user/
    }   
}
