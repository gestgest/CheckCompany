using UnityEngine;

public class MissionController : MonoBehaviour
{
    static MissionController instance;
    [SerializeField] private MissionSO [] missions;

    public MissionSO GetMission(int id)
    {
        return missions[id];
    }
}
