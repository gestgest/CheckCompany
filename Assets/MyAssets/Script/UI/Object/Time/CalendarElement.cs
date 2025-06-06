using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CalendarElement : MonoBehaviour
{
    Date date;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private MissionManagerSO _missionManagerSO;

    private void Awake()
    {
        date = new Date();
    }

    public void SetDay(int year, int month, int day)
    {

        dayText.color = Color.black;
        dayText.text = day.ToString();

        date.Year = year;
        date.Month = month;
        date.Day = day;
    }

    public void SetToday()
    {
        //폰트를 초록색으로
        dayText.color = Color.green;
    }

    //버튼 => 쿼리 전환
    public void QueryDate()
    {
        List<int> panel_index = new List<int>();

        panel_index.Add(1);
        panel_index.Add(2); //edit

        _missionManagerSO.SetCompleteDate(date);
        GamePanelManager.instance.SwitchingPanel(panel_index);
    }


}
