using UnityEngine;

/// <summary>
/// 직원이 실제로 앉는(=걸어가는) 지점. 의자 프리팹에만 붙인다.
///
/// 예전에는 PlaceableObject가 SeatPoint 필드를 들고 있었는데,
/// 화분이나 서랍장까지 전부 그 필드를 달고 다니는데도 실제로 쓰이는 건 의자뿐이었다.
/// 자리를 배정하는 단위는 컴퓨터(ObjectType.Computer)이고,
/// 그 컴퓨터가 올라간 책상에 붙어있는 의자가 앉을 곳이다 - WorkstationManagerSO.TryResolveSeat 참고.
///
/// _point를 비워두면 의자 자신의 Transform을 쓴다.
/// 의자 메시 안쪽에 파묻히는 등 위치를 조정해야 할 때만 'SeatPoint'라는 이름의
/// 빈 자식을 만들어두면 에디터에서 자동으로 꽂힌다.
/// </summary>
[RequireComponent(typeof(PlaceableObject))]
public class Seat : MonoBehaviour
{
    //이름 규칙만 지키면 프리팹마다 손으로 끌어다 넣지 않아도 된다
    private const string PointName = "SeatPoint";

    [SerializeField] private Transform _point;

    /// <summary>직원이 걸어갈 지점. 따로 지정하지 않았으면 의자 자신.</summary>
    public Transform Point => _point != null ? _point : transform;

    /// <summary>
    /// 의자의 자리 지점. Seat 컴포넌트가 없는 의자 프리팹은 의자 자신의 Transform을 쓴다
    /// (지금까지의 동작과 같다).
    /// </summary>
    public static Transform PointOf(PlaceableObject chair)
    {
        if (chair == null)
        {
            return null;
        }

        Seat seat = chair.GetComponent<Seat>();

        return seat != null ? seat.Point : chair.transform;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        AutoAssignPoint();
    }

    private void OnValidate()
    {
        AutoAssignPoint();
    }

    private void AutoAssignPoint()
    {
        if (_point == null)
        {
            _point = transform.Find(PointName);
        }
    }
#endif
}
