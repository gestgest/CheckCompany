using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//assign - 1) assigned, 2) employee 
public class AssignEmployeeElement : MonoBehaviour
{
    protected Employee employee;
    protected bool isSelected;

    private Image _iconCache;

    //Awake는 오브젝트가 처음 활성화될 때만 돈다.
    //꺼져있는 패널의 element에 접근하는 경로가 있어서 지연 초기화로 바꿈. (GetComponent는 비활성 오브젝트에서도 동작)
    protected Image _icon
    {
        get
        {
            if (_iconCache == null)
                _iconCache = GetComponent<Image>();
            return _iconCache;
        }
    }

    [SerializeField] protected CreateMissionManagerSO _createMissionManager;


    public void SetEmployee(Employee employee)
    {
        this.employee = employee;
    }

    public void SetEmployee(Employee employee, Sprite icon)
    {
        SetEmployee(employee);
        _icon.sprite = icon;
    }

    public virtual void SetEmployee(Employee employee, bool isSelected)
    {
        this.employee = employee;
        
        //add icon
        IsSelected = isSelected;
    }

    //button Fucntion
    public virtual void SwitchingIsSelcted()
    {
        //IsSelected = !IsSelected;
    }
    public virtual bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            //override
        }
    }

}