using System.Collections.Generic;
using UnityEngine;

public class RecruitPanel : Panel
{
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent
    List<GameObject> recruitmentObjects; //RecruitmentElement

    [Header("Manager")]
    [SerializeField] private RecruitmentManagerSO _recruitmentManagerSO;
    
    [Space]
    [Header("Listening to events")]
    //[Header("Broadcasting on events")]
    [SerializeField] private BoolEventChannelSO _isChangedEvent;
    private bool _isChangedRecruitments = true;

    void OnEnable()
    {
        if (_isChangedRecruitments)
        {
            AllRemoveRecruitmentObjects(); //all remove
            CreateRecruitmentObjects(); //recruitment load
            _isChangedRecruitments = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        Init();
        _isChangedEvent._onEventRaised += SetIsChanged;
    }

    private void OnDestroy()
    {
        _isChangedEvent._onEventRaised -= SetIsChanged;
    }


    //all create recruitment Objects
    private void CreateRecruitmentObjects()
    {
        foreach (Recruitment recruitment in _recruitmentManagerSO.GetRecruitments())
        {
            CreateRecruitmentObject(recruitment);
        }
    }
    
    //Panel안에 채용 목록 띄워주는 함수
    private void CreateRecruitmentObject(Recruitment r)
    {
        GameObject recruitmentObject = Instantiate(recruitmentPrefab, view.transform);
        RecruitmentElement recruitmentContent = recruitmentObject.GetComponent<RecruitmentElement>();

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i)
        recruitmentContent.Init(r); //여기에 multiLayoutGroup height값을 추가
            
        recruitmentObjects.Add(recruitmentObject);
        
        recruitmentContent.AllCreateApplicantObjects(); //draw
    }

    private void Init()
    {
        if (recruitmentObjects == null)
        {
            recruitmentObjects = new List<GameObject>();
        }
    }

    #region PROPERTY
    private void SetIsChanged(bool isChanged)
    {
        _isChangedRecruitments = isChanged;
    }

    public void AllRemoveRecruitmentObjects()
    {
        Init();
        for (int i = 0; i < recruitmentObjects.Count; i++)
        {
            Destroy(recruitmentObjects[i]);
        }
        recruitmentObjects.Clear();
    }
    #endregion

}
