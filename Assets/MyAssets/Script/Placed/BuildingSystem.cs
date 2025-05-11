using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem instance;

    public GridLayout gridLayout;
    private Grid grid;
    public Tilemap mainTilemap;
    public TileBase takenTile;
    [SerializeField] Transform internObjectList;

    [Header("Event")]
    [SerializeField] private GameObjectEventChannelSO _createEvent;
    
    

    public GameObject prefab1;
    [SerializeField] private PlaceableObject selectedObject;


    private void Awake()
    {
        instance = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        _createEvent._onEventRaised += CreateObject;
    }

    //어차피 안드로이드인데 키보드를 넣을 이유가 있나.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreateObject(prefab1);
        }

        if (!selectedObject)
        {
            return;
        }

        //놓는 함수
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PutOnObject();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(selectedObject.gameObject);
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

        //타일이 없으면  
        if (CheckTile(selectedObject))
        {
            //놓기 전에 물어보는 함수

            selectedObject.Place();
            Vector3Int startpos = gridLayout.WorldToCell(selectedObject.GetStartPosition());
            TakenArea(startpos, selectedObject.Size);

            //배치 하는 순간 조종 권한 제거  
            Destroy(selectedObject.gameObject.GetComponent<HandlingObject>());
            selectedObject = null;
        }
        else
        {
            Destroy(selectedObject.gameObject);
        }
    }

    #region Building Placement  

    public void InitWithObject(GameObject building)
    {
        //맨 처음 생성할때 0,0,0에 생성  => 마우스 위치에 생성으로 
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);

        GameObject obj = Instantiate(building, position, Quaternion.identity);

        obj.transform.parent = internObjectList;
        //생성된 오브젝트에 HandlingObject속성 추가  
        obj.AddComponent<HandlingObject>();
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

    //타일 색칠 함수  
    public void TakenArea(Vector3Int startpos, Vector3Int size)
    {
        for(int i = 0; i < size.z; i++)
        {
            for(int j = 0; j < size.x; j++)
            {
                mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), takenTile);
            }

        }
    }
    #endregion


}
