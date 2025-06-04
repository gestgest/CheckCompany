using UnityEngine;

public class RecruitPanel : Panel
{
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent

    [Header("Listening on events")]
    [SerializeField] private GameObjectEventChannelSO _getRecruitment;

    [Space]
    [Header("Broadcasting on events")]
    [SerializeField] private GameObjectEventChannelSO _addRecruitmentObject;

    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitmentObject(Recruitment r)
    {
         //가져오는 함수
        
        GameObject recruitmentObject = GameObject.Instantiate(recruitmentPrefab, view.transform);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i)
        recruitmentContent.Init(); //여기에 multiLayoutGroup height값을 추가
        recruitmentContent.SetRecruitment(r);
        
        //recruitmentObjects.Add(recruitmentObject);
        _addRecruitmentObject.RaiseEvent(recruitmentObject);
        
        recruitmentContent.SetApplicant(); //draw
    }
}
