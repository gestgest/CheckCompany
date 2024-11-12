using UnityEngine;
using UnityEngine.UI;

public class ApplicantPanel : Panel
{
    private Button xButton;
    private Button yButton;

    private int index; //직원 index

    void SetValue(int index)
    {
        this.index = index;
    }
    //X
    public void DeleteApplicant()
    {
        PanelManager.instance.Back_Nav_Panel();
        //지원자 제거 함수
    }

    //Y
    public void EmployApplicant()
    {
        PanelManager.instance.Back_Nav_Panel();
        
        //지원자 고용 함수
    }


    //V는 직원 합류 후 지원자 삭제
    //X는 지원자 삭제
}
