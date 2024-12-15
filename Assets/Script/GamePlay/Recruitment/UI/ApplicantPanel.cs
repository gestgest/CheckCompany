using UnityEngine;
using UnityEngine.UI;

public class ApplicantPanel : Panel
{
    //private Button xButton;
    //private Button yButton;

    private int applicant_id; //직원 index
    private int recruitment_id; //채용공고 index
    //private RecruitmentElement recruitmentElement; //지원자 리스트가 있음
    //전역으로 가져오는 RecruitmentController


    public void SetID(int applicant_id, int recruitment_id)
    {
        this.applicant_id = applicant_id;
        this.recruitment_id = recruitment_id;
    }

    //X
    public void DeleteApplicant()
    {
        //지원자 제거 함수
        RecruitmentElement recruitmentElement = RecruitmentController.instance.GetRecruitmentObject(recruitment_id).GetComponent<RecruitmentElement>();
        recruitmentElement.RemoveApplicant(applicant_id);

        PanelManager.instance.Back_Nav_Panel();

    }

    //Y
    public void EmployApplicant()
    {
        RecruitmentElement recruitmentElement = RecruitmentController.instance.GetRecruitmentObject(recruitment_id).GetComponent<RecruitmentElement>();
        //생성되는 함수
        IEmployee employee = recruitmentElement.GetApplicant(applicant_id);

        if(employee != null)
        {
            EmployeeController.instance.CreateEmployee(employee);
            recruitmentElement.RemoveApplicant(applicant_id);
        }
        PanelManager.instance.Back_Nav_Panel();
    }


    //V는 직원 합류 후 지원자 삭제
    //X는 지원자 삭제
}
