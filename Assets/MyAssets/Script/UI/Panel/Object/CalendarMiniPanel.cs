using TMPro;
using UnityEngine;

public class CalendarMiniPanel : MiniPanel
{
    private int year;
    private int month;

    [SerializeField] private TextMeshProUGUI year_text;
    [SerializeField] private TextMeshProUGUI month_text;
    [SerializeField] private CalendarPanel calendarPanel;

    private int Year
    {
        get => year;
        set
        {
            year = value;
            year_text.text = value.ToString();
        }
    }

    private int Month
    {
        get => month;
        set
        {
            month = value;
            if (month > 12)
            {
                month -= 12;
            }
            if (month < 1)
            {
                month += 12;
            }
            month_text.text = month.ToString();
        }
    }

    public void SetValue(int year, int month)
    {
        this.Year = year;
        this.Month = month;
        //CalendarPanel을 동기화?
    }

    //year (int )
    public void AddYear(int value)
    {
        this.Year += value;
    }

    //month (int )
    public void AddMonth(int value)
    {
        this.Month += value;
    }

    //확인
    public void SetDate()
    {
        calendarPanel.SetDate(year, month);
        OffPanel();
    }

}
