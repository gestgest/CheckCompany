using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager ui_manager;
    //int executive = 1; //임원수
    int money;
    //Reputation reputation = Reputation.single; //레벨 [명예]
    //int exp = 0;

    #region MonoBehaviour
    void Start()
    {
        //디버깅
        Money = 0;
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

    enum Reputation
    {
        single = 0, //혼자하는 느낌
        teamProject = 1, //조별수준
        club = 2, //동아리
        startup = 3, //스타트업 [동]
        //지역 대표
        //도 대표
        //국가 대표
        //대륙 대표
        //글로벌
    }
}
