using TMPro;
using UnityEngine;

public class CalendarElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;

    public void SetDay(int day)
    {
        dayText.color = Color.black;
        dayText.text = day.ToString();
    }

    public void SetToday()
    {
        //폰트를 초록색으로
        dayText.color = Color.green;
    }


}
