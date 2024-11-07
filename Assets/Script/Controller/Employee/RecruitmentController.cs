using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitmentController : MonoBehaviour
{
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects;
    int id = 0;
    
    [SerializeField] List<EmployeeSO> employeeTypes; 
    [SerializeField] private GameManager gameManager; //데이터베이스
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent
    [SerializeField] private TextMeshProUGUI costText;

    //채용 정보 [버튼을 누르면 함수를 호출해서 tmp처럼 대신 넣는 느낌]
    private int employeeTypeIndex = 0; //0,1,2,3
    private int level = 0; //0,1,2
    private int period = 1; //1 3 7
    private int cost; //코스트 => 게임 오브젝트도 가져와서 설정해야 할 거 같은데



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
            CreateRecruitment(r);
        }
    }

    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitment(Recruitment r)
    {
        GameObject recruitmentObject = Instantiate(recruitmentPrefab, Vector3.zero, Quaternion.identity);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i);
        recruitmentContent.SetRecruitment(employeeTypes[(int)(r.GetEmployeeType())].GetIcon(), r.GetDay(), 0, r.GetID());
        recruitmentObjects.Add(recruitmentObject);
        recruitmentObject.transform.SetParent(view.transform);
    }

    //추가하는 함수
    public void AddRecruitment()
    {
        Recruitment recruitment = new Recruitment();

        //버튼 정보 가져오기 디버깅
        recruitment.SetEmployeeType(employeeTypes[employeeTypeIndex].GetEmployeeType());
        recruitment.SetLevel(level);
        recruitment.SetDay(period);
        recruitment.SetID(id++);
        //SetCost()
        
        recruitments.Add(recruitment);

        CreateRecruitment(recruitment);
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


    #endregion
}
