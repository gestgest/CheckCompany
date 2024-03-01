using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager ui_manager;
    int executive = 1; //임원수
    int money;
    //int exp = 0;

    #region MonoBehaviour
    void Start()
    {
        Money = 0 ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Property
    public int Money {  
        get { return money; } 
        set { 
            money = value;
            ui_manager.SetMoneyText(value);
        } 
    }
    #endregion
}
