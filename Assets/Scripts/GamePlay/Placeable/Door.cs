using UnityEngine;

/// <summary>
/// 직원이 드나들 때 여닫히는 문. 문 프리팹(ObjectType.Door)에만 붙인다.
///
/// Animator 대신 코드로 돌린다. 문은 축 하나짜리 회전이라 클립과 컨트롤러까지 만드는 것이
/// 배보다 배꼽인 것도 있지만, 실제 이유는 두 가지다.
/// - 닫히는 도중에 다음 직원이 오면 그 각도에서 그대로 다시 열려야 한다 (클립은 처음부터 되감는다).
/// - 두 명이 같이 드나들 때 먼저 지나간 쪽이 문을 닫아버리면 안 된다 (아래 _holders).
///
/// 회전축은 door.fbx가 이미 부품별로 쪼개져 있어서(LP_Door_Slab / LP_Hinge_0..2 / LP_Jamb_*)
/// 경첩 노드의 위치를 그대로 쓴다. 문짝 자신의 피벗으로 돌리면 문짝 한가운데가 축이 되어
/// 반쪽이 벽을 뚫고 반대편으로 나간다.
///
/// Seat과 같은 규칙 - 이름만 맞으면 프리팹마다 손으로 끌어다 넣지 않아도 된다.
/// </summary>
[RequireComponent(typeof(PlaceableObject))]
public class Door : MonoBehaviour
{
    //door.fbx의 노드 이름. 문짝은 정확히 이 이름이고, 경첩은 0/1/2로 여러 개라 접두사로 찾는다.
    private const string SlabName = "LP_Door_Slab";
    private const string HingePrefix = "LP_Hinge";

    [SerializeField] private Transform _slab;  //실제로 돌아가는 문짝
    [SerializeField] private Transform _hinge; //회전축이 지나는 지점(경첩). 높이는 상관없고 XZ만 쓰인다

    [Header("Motion")]
    //열렸을 때의 각도. 부호를 뒤집으면(-90) 반대쪽으로 열린다.
    [SerializeField] private float _openAngle = 90f;

    //다 여는 데(또는 다 닫는 데) 걸리는 시간. 이 게임은 실제 1초가 게임 1시간이라
    //앉기 모션(2.2초)처럼 잡으면 문 여는 데만 두 시간을 쓰는 꼴이 된다.
    [SerializeField] private float _openDuration = 0.45f;

    //마지막 사람이 놓은 뒤 닫히기 시작할 때까지의 여유.
    //바로 닫으면 아직 문턱을 다 못 지난 직원의 등 뒤에서 문이 덮친다.
    [SerializeField] private float _closeDelay = 0.8f;

    //지금 이 문을 열어둔 채로 잡고 있는 직원 수. 0이 되어야 닫힌다.
    private int _holders;

    //_holders가 0이 된 시각 + _closeDelay. 이 시각 전까지는 잡은 사람이 없어도 열려 있다.
    private float _closeTime;

    private float _angle;

    //닫힌 상태의 문짝 로컬 트랜스폼. 여기에 회전을 얹어서 매 프레임 다시 계산한다.
    //RotateAround로 조금씩 더하면 여닫기를 반복하는 사이에 오차가 쌓여 문이 제자리를 벗어난다.
    private Quaternion _closedLocalRotation;
    private Vector3 _closedLocalPosition;
    private Vector3 _hingeLocalPosition;

    /// <summary>이 배치물의 문. Door가 안 붙어 있는 문 프리팹이면 null (그냥 안 여닫힐 뿐이다).</summary>
    public static Door Of(PlaceableObject placeable)
    {
        return placeable != null ? placeable.GetComponent<Door>() : null;
    }

    /// <summary>문을 열어둔 채로 잡는다. 잡은 사람이 있는 동안은 닫히지 않는다.</summary>
    public void Hold()
    {
        _holders++;
    }

    /// <summary>잡고 있던 손을 놓는다. 마지막 한 명이 놓으면 _closeDelay 뒤부터 닫히기 시작한다.</summary>
    public void Release()
    {
        _holders = Mathf.Max(0, _holders - 1);

        if (_holders == 0)
        {
            _closeTime = Time.time + _closeDelay;
        }
    }

    private void Awake()
    {
        AutoAssignParts();

        if (_slab == null)
        {
            Debug.LogWarning(
                $"[Door] '{name}' : 회전할 문짝('{SlabName}')을 찾지 못해 이 문은 여닫히지 않습니다. " +
                "모델을 갈아끼웠다면 문짝 오브젝트를 _slab에 직접 꽂아주세요.",
                this);

            enabled = false;
            return;
        }

        _closedLocalRotation = _slab.localRotation;
        _closedLocalPosition = _slab.localPosition;

        //경첩을 못 찾으면 문짝 자기 피벗을 축으로 돈다. 문짝이 통째로 하나인 모델이면 그게 맞는 동작이고,
        //아니면 눈에 띄게 이상하게 돌아가므로 어느 쪽이든 바로 알아챌 수 있다.
        _hingeLocalPosition = _hinge != null && _slab.parent != null
            ? _slab.parent.InverseTransformPoint(_hinge.position)
            : _closedLocalPosition;
    }

    private void Update()
    {
        float target = _holders > 0 || Time.time < _closeTime ? _openAngle : 0f;

        if (Mathf.Approximately(_angle, target))
        {
            return;
        }

        //각도를 목표로 끌고 갈 뿐이라 닫히는 도중에 다시 열려도 지금 각도에서 이어진다
        float degreesPerSecond = Mathf.Abs(_openAngle) / Mathf.Max(_openDuration, 0.01f);

        _angle = Mathf.MoveTowards(_angle, target, degreesPerSecond * Time.deltaTime);

        ApplyAngle();
    }

    /// <summary>지금 각도를 문짝 트랜스폼에 반영한다. 경첩을 축으로 도는 것이 여기서 나온다.</summary>
    private void ApplyAngle()
    {
        //문틀은 y축으로만 돌아가 있으므로 부모 로컬 공간의 up이 곧 경첩 축이다
        Quaternion turn = Quaternion.AngleAxis(_angle, Vector3.up);

        _slab.localRotation = turn * _closedLocalRotation;
        _slab.localPosition = _hingeLocalPosition + turn * (_closedLocalPosition - _hingeLocalPosition);
    }

    private void AutoAssignParts()
    {
        if (_slab == null)
        {
            _slab = FindDescendant(SlabName);
        }

        if (_hinge == null)
        {
            //경첩은 위아래로 여러 개지만 전부 같은 모서리에 붙어 있어서 XZ는 같다. 아무거나 하나면 된다.
            _hinge = FindDescendant(HingePrefix);
        }
    }

    /// <summary>이름이 prefix로 시작하는 자손을 찾는다. 문틀 안쪽에 한 겹 더 들어가 있어도 찾도록 재귀로 본다.</summary>
    private Transform FindDescendant(string prefix)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != transform && children[i].name.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return children[i];
            }
        }

        return null;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        AutoAssignParts();
    }

    private void OnValidate()
    {
        AutoAssignParts();
    }
#endif
}
