using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TodoMissionElementUI : MonoBehaviour
{
    private Todo_Mission todoMission;

    [SerializeField] private TextMeshProUGUI title; 
    [SerializeField] private Image icon; 
    
    public void SetMission(Todo_Mission todoMission)
    {
        this.todoMission = todoMission;

        title.text = todoMission.GetName();
        icon.sprite = todoMission.GetIcon();
        
        //버튼 이벤트는 따로
    }

    public void AddEventListener(UnityAction listener)
    {
        Button button = this.GetComponent<Button>();
        button.onClick.AddListener(listener);
    }
}
