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

    private void Start()
    {
        recruitments = new List<Recruitment>();
        recruitmentObjects = new List<GameObject>();
        //recruitments 가져오는 함수()
        Recruitment r = new Recruitment();
        r.SetDay(0);
        r.SetEmployeeType(EmployeeType.Developer);
        recruitments.Add(r);

        CreateRecruitments();
        //ShowRecruitments();
    }

    private void CreateRecruitments()
    {
        for(int i = 0; i < recruitments.Count; i++)
        {
            Recruitment r = recruitments[i];
            GameObject recruitmentObject = Instantiate(recruitmentPrefab);
            RecruitmentContent recruitmentContent = recruitmentObject.GetComponent<RecruitmentContent>();

            //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize());
            recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), 0);
            recruitmentObjects.Add(recruitmentObject);
            recruitmentObject.transform.SetParent(this.gameObject.transform);
        }
    }

    //대충 Panel안에 채용 목록 띄워주는 함수
    private void ShowRecruitments()
    {
        //대충 recruitments for문
    }
}
