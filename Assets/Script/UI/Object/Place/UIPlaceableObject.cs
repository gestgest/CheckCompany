using System;
using UnityEngine;

public class UIPlaceableObject : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _okButton;
    [SerializeField] private GameObject _denyButton;
    
    [SerializeField] private PlacedObjectManager _placedObjectManager; 

    private void Start()
    {
        _placedObjectManager.SetHandlingObjectProperties(_camera, _okButton, _denyButton);
        // if (_placedObjectManager == null)
        // {
        //     Debug.LogError("_placedObjectManager == null");
        // }
        // _placedObjectManager.SetCamera(_camera);
        // _placedObjectManager.SetOkButton(_okButton);
        // _placedObjectManager.SetDenyButton(_denyButton);
    }
}
