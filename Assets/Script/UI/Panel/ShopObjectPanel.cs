using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 카테고리 탭 하나(의자, 책상 ...). 자기 ObjectType에 해당하는 오브젝트만 칸으로 깔아준다.
/// 칸을 누르면 그 오브젝트를 손에 들고 배치 모드로 들어간다.
///
/// ShopPanel의 자식으로 붙는다 - ShopPanel이 자식 패널을 CategoryPanel로 캐스팅해서
/// 상단 카테고리 버튼을 만들기 때문에, 반드시 CategoryPanel을 상속해야 한다.
///
/// 탭을 하나 더 만들려면 이 스크립트를 붙인 패널을 하나 더 두고 _type만 바꾸면 된다.
/// </summary>
public class ShopObjectPanel : CategoryPanel
{
    [Header("Shop")]
    //이 탭이 맡을 카테고리
    [SerializeField] private ObjectType _type;

    [SerializeField] private ObjectAssetsSO _objectAssetsSO;

    [SerializeField] private GameObject _elementPrefab;

    //칸이 쌓일 자리(ScrollView의 Content). 비어있으면 이 패널 자신에 붙인다.
    [SerializeField] private Transform _elementParent;

    [Header("Manager")]
    [SerializeField] private PlacedObjectManager _placedObjectManager;

    private readonly List<ShopObjectElement> _elements = new List<ShopObjectElement>();

    //칸은 한 번만 만든다. 패널은 열고 닫을 때마다 SetActive가 오가기 때문에
    //열 때마다 다시 만들면 목록이 두 배씩 늘어난다.
    private bool _isBuilt;

    public override void OnPanel()
    {
        //base.OnPanel()이 SetActive(true)를 하지만 Start()는 그 다음에나 돈다.
        //Start()에서 만들면 처음 열었을 때 한 프레임 빈 목록이 보인다.
        BuildElements();

        base.OnPanel();
    }

    private void BuildElements()
    {
        if (_isBuilt)
        {
            return;
        }

        //하나라도 비어 있으면 칸이 안 생기는데, 화면에는 그냥 빈 탭으로만 보여서 원인을 찾기 어렵다
        if (_objectAssetsSO == null || _elementPrefab == null || _placedObjectManager == null)
        {
            Debug.LogError(
                $"[ShopObjectPanel] '{name}' : _objectAssetsSO / _elementPrefab / _placedObjectManager를 " +
                "인스펙터에서 넣어주세요.",
                this);
            return;
        }

        Transform parent = _elementParent != null ? _elementParent : transform;
        List<ObjectSO> objects = _objectAssetsSO.GetObjects(_type);

        for (int i = 0; i < objects.Count; i++)
        {
            CreateElement(objects[i], parent);
        }

        //카테고리는 만들었는데 그 종류의 ObjectSO를 아직 안 만든 상태
        if (objects.Count == 0)
        {
            Debug.LogWarning(
                $"[ShopObjectPanel] '{name}' : {_type} 카테고리에 해당하는 ObjectSO가 없습니다.",
                this);
        }

        _isBuilt = true;
    }

    private void CreateElement(ObjectSO objectSO, Transform parent)
    {
        GameObject obj = Instantiate(_elementPrefab, parent);
        ShopObjectElement element = obj.GetComponent<ShopObjectElement>();

        if (element == null)
        {
            Debug.LogError(
                $"[ShopObjectPanel] '{name}' : _elementPrefab에 ShopObjectElement가 없습니다.",
                _elementPrefab);
            Destroy(obj);
            return;
        }

        element.Init(objectSO, _placedObjectManager);
        _elements.Add(element);
    }
}
