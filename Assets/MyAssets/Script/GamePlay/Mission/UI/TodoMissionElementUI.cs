using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TodoMissionElementUI : MonoBehaviour
{
    private Todo_Mission todoMission;

    [SerializeField] private TextMeshProUGUI title; 
    [SerializeField] private Image icon; 
    
    void SetMission(Todo_Mission todoMission)
    {
        this.todoMission = todoMission;

        title.text = todoMission.GetName();
        icon.sprite = todoMission.GetIcon();
    }
}
