using TMPro;
using UnityEngine;

public class CalendarMiniPanel : MiniPanel
{
    [SerializeField] private TextMeshProUGUI year_text;
    [SerializeField] private TextMeshProUGUI month_text;

    private int year;
    private int month;

    void SetValue(int year, int month)
    {
        this.year = year;
        this.month = month;

        year_text.text = year.ToString();
        month_text.text = month.ToString();

        //CalendarPanel을 동기화?
    }

}
