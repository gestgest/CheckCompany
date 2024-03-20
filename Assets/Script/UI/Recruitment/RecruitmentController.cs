using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitmentController : MonoBehaviour
{
    //채용 리스트
    List<Recruitment> recruitments;
    List<GameObject> recruitmentObjects;

    [SerializeField] List<Sprite> employeeTypeIcons; //아이콘
    [SerializeField] private GameManager gameManager; //데이터베이스
    EmployeeType employeeType = default; //나중에 타입 넣을때 사용
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view;

    private void Start()
    {
        recruitments = new List<Recruitment>();
        recruitmentObjects = new List<GameObject>();

        //recruitments 가져오는 함수()
        Recruitment r = new Recruitment();
        r.SetEmployeeType(EmployeeType.Developer);
        r.SetDay(0);
        r.SetID(0);

        recruitments.Add(r);

        InitRecruitments(); //초기 설정
        //ShowRecruitments();
    }

    //pool 안 만들거임
    private void InitRecruitments()
    {
        for(int i = 0; i < recruitments.Count; i++)
        {
            Recruitment r = recruitments[i];
            
            GameObject recruitmentObject = Instantiate(recruitmentPrefab, Vector3.zero, Quaternion.identity);
            RecruitmentContent recruitmentContent = recruitmentObject.GetComponent<RecruitmentContent>();

            //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i);
            recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), 0, i);
            recruitmentObjects.Add(recruitmentObject);
            recruitmentObject.transform.SetParent(view.transform);
        }
    }

    //대충 Panel안에 채용 목록 띄워주는 함수
    private void ShowRecruitments()
    {
        //대충 recruitments for문
    }
    public void AddRecruitment(Recruitment recruitment)
    {

    }
}
