using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplicantElement : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name_text;
    [SerializeField] private TextMeshProUGUI age_text;
    [SerializeField] private TextMeshProUGUI salary_text;
    [SerializeField] private TextMeshProUGUI careerPeriod_text;
    private ApplicantPanel applicantPanel;

    //int recruitment_id;

    private long applicant_id;

    private void Start()
    {
        //이거를 바꿔야 함
        applicantPanel = GamePanelManager.instance.GetPanel(GetApplicantPanelIndexList()) as ApplicantPanel;
    }

    // Init 급
    public void SetValue(Employee employee, int recruitment_id)
    {
        
        //icon
        name_text.text = employee.Name;
        age_text.text = employee.Age.ToString() + "살";
        salary_text.text = (employee.Salary / 10000).ToString() + "만원";
        careerPeriod_text.text = employee.CareerPeriod.ToString() + "개월";

        Button button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        //버튼 누르면 이동
        button.onClick.AddListener(() =>
        {
            PanelManager.instance.SwitchingSubPanel(true, GetApplicantPanelIndexList());
            applicantPanel.SetID(employee.ID, recruitment_id);
        });
    }

    private List<int> GetIndexList()
    {
        List<int> indexList = new List<int>();
        
        indexList.Add(0);
        indexList.Add(2);
        
        return indexList;
    }
    private List<int> GetApplicantPanelIndexList()
    {
        List<int> indexList = new List<int>();
        
        indexList.Add(0);
        indexList.Add(2);
        indexList.Add(0);
        
        return indexList;
    }
}