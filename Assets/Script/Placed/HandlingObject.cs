using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandlingObject : MonoBehaviour
{
    private GameObject _okButton;
    private GameObject _denyButton;
    private GameObject _deleteButton;
    private Transform _cameraTransform;

    //이미 배치가 끝난 오브젝트를 이동중일 때만 삭제할 수 있다.
    //상점에서 막 꺼내 아직 놓지도 않은 오브젝트는 취소(deny)가 곧 삭제라 버튼을 띄울 이유가 없다.
    private bool _canDelete;

    //오브젝트를 기준으로 버튼을 놓을 위치(월드 오프셋).
    //런타임에 AddComponent로 붙기 때문에 인스펙터 값이 아니라 이 기본값이 그대로 쓰인다.
    private static readonly Vector3 OkButtonOffset = new Vector3(-1.0f, 0.0f, -3.0f);
    private static readonly Vector3 DenyButtonOffset = new Vector3(-3.0f, 0.0f, -1.0f);
    private static readonly Vector3 DeleteButtonOffset = new Vector3(-3.0f, 0.0f, -3.0f);

    
    private VoidEventChannelSO _takenAreaEvent;
    private Vector3TransformChannelSO _snapCoordinateToGrid;
    
    
    [SerializeField] private Vector2 _screenEdge;

    //fix : 오브젝트를 옮기고 버튼을 누를때 버튼을 누르면 그 위치로 순간이동을 먼저하고 놓아지는 버그
    //ㄴ 그거를 막는 변수
    private bool _isPressStartedOverUI;

    //UI 레이캐스트 결과 재사용(매 프레임 GC 방지)
    private static readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();


    //down
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI 위를 눌렀다면 이 제스처는 통째로 무시한다
            _isPressStartedOverUI = IsPointerOverUI(Input.mousePosition);
            if (_isPressStartedOverUI)
            {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            MoveObject(mousePos);
            OffButton();
        }
        if (Input.GetMouseButtonUp(0))
        {
            //UI에서 시작한 제스처면 버튼을 끈 적이 없으므로 다시 켤 필요도 없다
            if (_isPressStartedOverUI)
            {
                _isPressStartedOverUI = false;
                return;
            }

            OnButton();
        }
    }

    //OnMouseDrag()
    private void OnMouseDrag()
    {
        //버튼 위에서 시작한 드래그로는 오브젝트가 따라오면 안 된다
        if (_isPressStartedOverUI)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        MoveObject(mousePos);
        MoveCamera(mousePos); //화면 움직이는 함수
    }

    /// <summary>
    /// 화면 좌표가 UI 위인지 확인한다.
    ///
    /// EventSystem.IsPointerOverGameObject()는 인자가 없으면 마우스 포인터(id -1) 기준이라
    /// 안드로이드 터치(fingerId 0,1,...)에서는 항상 false가 나온다.
    /// fingerId를 넘기는 오버로드도 터치가 시작된 프레임에는 EventSystem 갱신 순서에 따라 결과가 흔들리므로,
    /// 실행 순서에 영향받지 않도록 GraphicRaycaster로 직접 쏜다.
    /// </summary>
    private static bool IsPointerOverUI(Vector3 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiRaycastResults);

        for (int i = 0; i < _uiRaycastResults.Count; i++)
        {
            //PhysicsRaycaster가 카메라에 붙어 있으면 3D 오브젝트도 결과에 섞이므로 UI(Canvas)만 센다
            if (_uiRaycastResults[i].module is GraphicRaycaster)
            {
                return true;
            }
        }

        return false;
    }
    
    public void Init(
        GameObject okButton,
        GameObject denyButton,
        GameObject deleteButton,
        GameObject camera,
        VoidEventChannelSO takenAreaEvent,
        Vector3TransformChannelSO snapCoordinateToGrid,
        bool canDelete)
    {
        this._okButton = okButton;
        this._denyButton = denyButton;
        this._deleteButton = deleteButton;
        this._canDelete = canDelete;

        //camera null
        this._cameraTransform = camera.GetComponent<Transform>();
        
        _takenAreaEvent = takenAreaEvent;
        _snapCoordinateToGrid = snapCoordinateToGrid;
    }
    
    
    private void MoveObject(Vector3 mousePos)
    {
        //화면 포지션 값을 타일맵 좌표로 변환 => 기다려라 
        transform.position = _snapCoordinateToGrid.RaiseEvent(GetObjectPos(mousePos));

        //x축 => 왼쪽 아래, z축 => 오른쪽 아래
        _takenAreaEvent.RaiseEvent();   //색칠
    }

    private void OnButton()
    {
        PlaceButton(_okButton, OkButtonOffset);
        PlaceButton(_denyButton, DenyButtonOffset);

        //이동 모드가 아니면 삭제 버튼은 계속 숨겨둔다
        if (_canDelete)
        {
            PlaceButton(_deleteButton, DeleteButtonOffset);
        }
    }
    private void OffButton()
    {
        SetButtonActive(_okButton, false);
        SetButtonActive(_denyButton, false);
        SetButtonActive(_deleteButton, false);
    }

    /// <summary>버튼을 켜고 오브젝트 옆(월드 오프셋 위치)으로 옮긴다.</summary>
    private void PlaceButton(GameObject button, Vector3 worldOffset)
    {
        if (button == null)
        {
            return;
        }

        button.SetActive(true);
        button.transform.position = Camera.main.WorldToScreenPoint(transform.position + worldOffset);
    }

    private static void SetButtonActive(GameObject button, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        button.SetActive(isActive);
    }
    
    ///<summary>
    /// 커서 아래 바닥 좌표를 구한다.
    /// 드래그 중인 오브젝트는 항상 커서 바로 아래(=카메라에 제일 가까움)에 있어서
    /// 단순 Raycast를 쓰면 자기 자신에게 맞아버린다. 그래서 자기 자신은 건너뛴다.
    ///</summary>
    private Vector3 GetObjectPos(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        float nearest = float.MaxValue;
        Vector3 result = Vector3.zero;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            //자기 자신(과 자식)에 맞은 것은 무시
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (!(hit.collider is BoxCollider) || hit.distance >= nearest)
            {
                continue;
            }

            nearest = hit.distance;
            result = hit.point;
            found = true;
        }

        //바닥을 못 찾았으면 제자리를 유지한다.
        //(기존에는 Vector3.zero를 돌려줘서 오브젝트가 월드 원점으로 순간이동했다)
        return found ? result : transform.position;
    }

    private void MoveCamera(Vector3 mousePos)
    {
        Vector3 mouseDir = Vector3.zero;
        
        // <- ^
        //왼쪽
        if (mousePos.x >= Screen.width - _screenEdge.x)
        {
            mouseDir += Vector3.left;
            mouseDir += Vector3.forward;
        }
        else if (mousePos.x <= _screenEdge.x)
        {
            mouseDir += Vector3.right;
            mouseDir += Vector3.back;
        }
        if (mousePos.y >= Screen.height - _screenEdge.y)
        {
            mouseDir += Vector3.left;
            mouseDir += Vector3.back;
        }
        else if (mousePos.y <= _screenEdge.y)
        {
            mouseDir += Vector3.right;
            mouseDir += Vector3.forward;
        }

        Vector3 newpos = _cameraTransform.position + mouseDir.normalized * Time.deltaTime;

        _cameraTransform.position = newpos;
    }
    
}
