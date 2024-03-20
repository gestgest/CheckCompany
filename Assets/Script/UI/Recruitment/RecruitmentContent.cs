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
    public int ID { get; set; }

    private void Start()
    {
        //icon.sprite
    }
    public void SetRecruitment(Sprite sprite, int day, int size, int id)
    {
        SetIcon(sprite);
        SetDDay(day);
        SetRecruitmentNumber(size);
        ID = id;
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
