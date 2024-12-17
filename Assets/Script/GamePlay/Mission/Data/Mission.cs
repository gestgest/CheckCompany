using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission : MonoBehaviour
{
    [SerializeField]private MissionSO m_SO;

    //달성률
    int goal;
    //int mission_id; //어떤 미션을 가지고 있는지 => 나중에 미션 컨트롤러에서 m_SO를 가져오기 위한

    #region SERVER
    

    //서버에서 가져온 JSON 타입을 Mission에 넣기
    void SetMissionFromJSON(Dictionary<string, object> mission)
    {
        //mission["goal"]


    }


    //
    #endregion
}