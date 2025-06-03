using UnityEngine;

public class RecruitPanel : Panel
{
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent


    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitmentObject(Recruitment r)
    {
        GameObject recruitmentObject = GameObject.Instantiate(recruitmentPrefab, view.transform);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

    }
}
