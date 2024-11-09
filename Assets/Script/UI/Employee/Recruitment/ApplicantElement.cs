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

    //
    public void SetValue(IEmployee employee)
    {
        //icon
        name_text.text = employee.Name;
        age_text.text = employee.Age.ToString() + "살";
        salary_text.text = employee.Salary.ToString() + "만원";
        careerPeriod_text.text = employee.CareerPeriod.ToString() + "개월";
    }
}
