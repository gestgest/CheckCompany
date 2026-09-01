using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//놓는 물체
[CreateAssetMenu(fileName = "ObjectSO", menuName = "ScriptableObject/ObjectSO")]
public class ObjectSO : ScriptableObject
{
    [SerializeField] private string object_name;
    [SerializeField] private int width;
    [SerializeField] private int length;
    [SerializeField] private int money;

    [Header("Shop")]
    //상점에서 어느 카테고리 탭에 뜰지
    [SerializeField] private ObjectType type;

    //상점 칸에 뜨는 그림
    [SerializeField] private Sprite icon;

    //실제로 씬에 놓이는 프리팹. PlaceableObject가 붙어 있어야 한다.
    //차지하는 칸 수는 프리팹의 BoxCollider에서 계산되므로 위의 width/length는 표시용이다.
    [SerializeField] private GameObject prefab;

    public string GetName()
    {
        return object_name;
    }

    public ObjectType GetObjectType()
    {
        return type;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }

    public int GetMoney()
    {
        return money;
    }
}

//상점 카테고리. 탭 하나가 이 값 하나를 맡는다.
//값을 지우거나 순서를 바꾸면 이미 만들어 둔 ObjectSO 에셋이 엉뚱한 카테고리로 간다 - 뒤에 추가만 할 것.
public enum ObjectType
{
    Chair = 0,
    Table = 1,
    Cabinet = 2,
    Plant = 3,
    Etc = 4,
}
