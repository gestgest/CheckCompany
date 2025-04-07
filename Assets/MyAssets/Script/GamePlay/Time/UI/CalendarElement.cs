using TMPro;
using UnityEngine;

public class CalendarElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;

    public void SetDay(int day)
    {
        dayText.text = day.ToString();
    }


}
