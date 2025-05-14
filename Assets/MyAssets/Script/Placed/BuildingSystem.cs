using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private TileBase takenTile;
    [SerializeField] Transform internObjectList;

    [Header("selected")]
    //selected
    [SerializeField] private PlaceableObject selectedObject;
    [SerializeField] private Transform _tmp_parent;
    [SerializeField] private GameObject _okButton;
    [SerializeField] private GameObject _denyButton;

    [Header("Event")]
    [SerializeField] private GameObjectEventChannelSO _createEvent;
    [SerializeField] private VoidEventChannelSO _takenAreaEvent;
    [SerializeField] private Vector3TransformChannelSO _gridEvent;
    
    private bool isFirst = true; 
    private Vector3Int startpos;
    private Vector3Int object_size;

    private void Awake()
    {
        grid = gridLayout.gameObject.GetComponent<Grid>();

    }

    private void OnEnable()
    {
        _createEvent._onEventRaised += CreateObject;
        _takenAreaEvent._onEventRaised += TakenArea;
        _gridEvent._onEventRaised += SnapCoordinateToGrid;
    }
    private void OnDisable()
    {
        _createEvent._onEventRaised -= CreateObject;
        _takenAreaEvent._onEventRaised -= TakenArea;
        _gridEvent._onEventRaised -= SnapCoordinateToGrid;
    }

    //어차피 안드로이드인데 키보드를 넣을 이유가 있나.
    private void Update()
    {
        if (!selectedObject)
        {
            return;
        }

        //놓는 함수v
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PutOnObject();
        }
        //해체하는 함수x
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TakeOffObject();
        }
    }

    //건물 만드는 함수
    public void CreateObject(GameObject obj)
    {
        //선택된 오브젝트가 있다면
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }
        InitWithObject(obj);
    }

    //건물 놓는 함수
    public void PutOnObject()
    {
        //겹치는 타일이 없으면
        if (CheckTile(selectedObject))
        {
            //놓기 전에 물어보는 함수
            selectedObject.Place();

            startpos = gridLayout.WorldToCell(selectedObject.GetStartPosition());
            ClearArea();
            //TakenArea(startpos, selectedObject.Size);

            TakeOffBuilding();
            //배치 하는 순간 조종 권한 제거  
            Destroy(selectedObject.gameObject.GetComponent<HandlingObject>());
            selectedObject = null;
        }
        //겹치면 그냥 진행
    }

    //안 놓는 함수
    public void TakeOffObject()
    {
        ClearArea();
        TakeOffBuilding();
        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    private void TakeOffBuilding()
    {
        //버튼 안 보이게
        _okButton.SetActive(false);
        _denyButton.SetActive(false);
    }

    #region Building Placement  

    public void InitWithObject(GameObject building)
    {
        //맨 처음 생성할때 0,0,0에 생성  => 마우스 위치에 생성으로 
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);

        GameObject obj = Instantiate(building, position, Quaternion.identity);

        obj.transform.parent = internObjectList;
        //생성된 오브젝트에 HandlingObject속성 추가  

        _okButton.SetActive(true);
        _denyButton.SetActive(true);
        obj.AddComponent<HandlingObject>().Init(
            _okButton.transform,
            _denyButton.transform, 
            _takenAreaEvent,
            _gridEvent
        );
        
        selectedObject = obj.GetComponent<PlaceableObject>();
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    //area는 범위, tilemap
    private static TileBase[] GetTileBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y];
        int count = 0;

        foreach (Vector3Int v in area.allPositionsWithin)
        {
            Debug.Log(v);
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[count] = tilemap.GetTile(pos);
            count++;
        }

        return array;
    }

    //타일이 비어있는지  
    public bool CheckTile(PlaceableObject ob)
    {
        Vector3Int position = gridLayout.WorldToCell(ob.GetStartPosition());

        //타일 베이스 [타일 가져오기]  
        for(int i = 0; i < ob.Size.z;i++)
        {
            for(int j = 0; j < ob.Size.x;j++)
            {
                TileBase b = mainTilemap.GetTile(new Vector3Int(position.x + j, position.z + i,0));
                //b에 takenTile가 있다면???  
                if (b == takenTile)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary> 타일 채우는 함수</summary>
    /// <param name="startpos"></param>
    /// <param name="size"></param>
    private void TakenArea(Vector3Int startpos, Vector3Int size)
    {
        this.startpos = startpos;
        this.object_size = size;
        for(int i = 0; i < size.z; i++)
        {
            for(int j = 0; j < size.x; j++)
            {
                mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), takenTile);
            }
        }
    }
    
    /// <summary>
    /// 놓는 함수, 근데 너무 비효율적이지 않을까
    /// </summary>
    public void TakenArea()
    {
        ClearArea();
        Vector3Int startpos = gridLayout.WorldToCell(selectedObject.GetStartPosition());
        TakenArea(startpos, selectedObject.Size);
    }

    private void ClearArea()
    {
        if (isFirst)
        {
            isFirst = false;
            return;
        }
        for(int i = 0; i < object_size.z; i++)
        {
            for(int j = 0; j < object_size.x; j++)
            {
                mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), null);
            }
        }
    }
    #endregion


}
