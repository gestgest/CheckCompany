using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmallMissionElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Toggle my_toggle;
    [SerializeField] private Gauge gauge;

    public void SetGague(Gauge gauge)
    {
        this.gauge = gauge;
    }

    ///<summary>대충 토글 상태에 따라 폰트 바꾸는 코드 </summary>
    public void UpdateSmallMissionStatus()
    {
        if (my_toggle.isOn)
        {
            title.fontStyle = FontStyles.Italic;
            title.color = Color.gray;
            //gage 값 전달
            gauge.AddValue(1);
            if (gauge.GetValue() >= gauge.GetMaxValue())
            {
                //미션 다 했다는 이야기
            }
        }
        else
        {
            title.fontStyle = FontStyles.Normal;
            title.color = Color.black;
            //gage 값 전달
            gauge.AddValue(-1);
        }
        //취소선은 <s> 이걸로 하라는데
    }
}