using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeNameSO", menuName = "ScriptableObject/EmployeeNameSO")]
public class EmployeeNameSO : ScriptableObject
{
    //나중에 sprite + string이름 들어있는 구조체 구조로 만들어야 한다
    [SerializeField] private string [] male_names;
    [SerializeField] private string [] female_names;

    public string GetMaleName(int index)
    {
        return male_names[index];
    }
    public string GetFemaleName(int index)
    {
        return female_names[index];
    }
    public string GetRandomName()
    {
        int male_size = male_names.Length;
        int female_size = female_names.Length;

        int index = Random.Range(0, male_size + female_size);
        if(index >= male_size)
        {
            index -= male_size;
            return female_names[index];
        }
        return male_names[index];
    }
}
