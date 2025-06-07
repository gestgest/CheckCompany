using System;
using UnityEngine;

public class UIPlaceableObject : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _okButton;
    [SerializeField] private GameObject _denyButton;
    
    [Header("Broadcasting on events")]
    [SerializeField] private GameObjectEventChannelSO _SetOkButtonEvent;
    [SerializeField] private GameObjectEventChannelSO _SetDenyButtonEvent;
    [SerializeField] private GameObjectEventChannelSO _SetCameraEvent;

    private void Start()
    {
        _SetOkButtonEvent.RaiseEvent(_okButton);        
        _SetDenyButtonEvent.RaiseEvent(_denyButton);        
        _SetCameraEvent.RaiseEvent(_camera);        
    }
}
