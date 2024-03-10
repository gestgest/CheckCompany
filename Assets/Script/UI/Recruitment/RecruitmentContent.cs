using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//채용 칸 스크립트
public class RecruitmentContent : MonoBehaviour
{
    //모델
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI dDayText;
    [SerializeField] private TextMeshProUGUI RecruitmentNumber_Text;
    private void Start()
    {
        //icon.sprite
    }
    public void SetRecruitment(Sprite sprite, int day, int size)
    {
        SetIcon(sprite);
        SetDDay(day);
        SetRecruitmentNumber(size);
    }

    private void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    private void SetDDay(int day)
    {
        dDayText.text = day.ToString();
    }
    private void SetRecruitmentNumber(int size)
    {
        RecruitmentNumber_Text.text = size.ToString();
    }
}
