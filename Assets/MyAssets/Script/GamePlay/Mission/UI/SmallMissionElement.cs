using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmallMissionElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;

    [SerializeField]  private Toggle my_toggle;

    ///<summary>대충 토글 상태에 따라 폰트 바꾸는 코드 </summary>
    public void UpdateSmallMissionStatus()
    {
        if (my_toggle.isOn)
        {
            title.fontStyle = FontStyles.Italic;
            title.color = Color.gray;
        }
        else
        {
            title.fontStyle = FontStyles.Normal;
            title.color = Color.black;
        }
        //<s> 이걸로 하라는데
    }
}
