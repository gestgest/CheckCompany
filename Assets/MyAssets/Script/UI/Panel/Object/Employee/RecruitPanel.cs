using System.Collections.Generic;
using UnityEngine;

public class RecruitPanel : Panel
{
    [SerializeField] private GameObject recruitmentPrefab;
    [SerializeField] private GameObject view; //parent
    List<RecruitmentElement> recruitmentObjects; //RecruitmentElement

    [SerializeField] private MultiLayoutGroup _rootLayoutGroup;
    
    [Header("Manager")]
    [SerializeField] private RecruitmentManagerSO _recruitmentManagerSO;
    
    [Space]
    [Header("Listening to events")]
    //[Header("Broadcasting on events")]
    [SerializeField] private BoolEventChannelSO _isChangedEvent;
    private bool _isChangedRecruitments = true;
    
    private static readonly int RECRUITMENT_HEIGHT = 150; //recruitmentElement height : 100 + space : 50

    void OnEnable()
    {
        SetUI();
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

    private void SetUI()
    {
        Init();
        if (_isChangedRecruitments)
        {
            if (_rootLayoutGroup.GetIsInit())
            {
                _rootLayoutGroup.Init();
            }
                
            _rootLayoutGroup.SetParentZeroHeight();
            AllRemoveRecruitmentObjects(); //all remove
            CreateRecruitmentObjects(); //recruitment load
            _isChangedRecruitments = false;
        }
    }
    
    private void AllRemoveRecruitmentObjects()
    {
        for (int i = 0; i < recruitmentObjects.Count; i++)
        {
            Destroy(recruitmentObjects[i].gameObject);
        }

        recruitmentObjects.Clear();
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
        recruitmentContent.SetEmployee(r); //여기에 multiLayoutGroup height값을 추가
        _rootLayoutGroup.AddHeight(RECRUITMENT_HEIGHT);
        
        recruitmentObjects.Add(recruitmentContent);
    }

    private void Init()
    {
        if (recruitmentObjects == null)
        {
            recruitmentObjects = new List<RecruitmentElement>();
        }
    }

    
    
    #region PROPERTY
    private void SetIsChanged(bool isChanged)
    {
        _isChangedRecruitments = isChanged;
    }

    #endregion

}
