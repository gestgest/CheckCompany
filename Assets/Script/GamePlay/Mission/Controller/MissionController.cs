using UnityEngine;

public class MissionController : MonoBehaviour
{
    public static MissionController instance;
    [SerializeField] private MissionSO [] missions;

    public MissionSO GetMission(int id)
    {
        return missions[id];
    }

    public MissionSO [] GetMissions()
    {
        return missions;
    }
    public int GetMissionSize()
    {
        return missions.Length;
    }
        
}
