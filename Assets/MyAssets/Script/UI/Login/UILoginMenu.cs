using System;
using UnityEngine;
using TMPro;


//나중에 AuthManager에서 UI만 가져와야 함
public class UILoginMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField _loginEmailTextField;
    [SerializeField] private TMP_InputField _loginPasswordTextField;

    [Space]
    [SerializeField] private TMP_InputField _registerNameTextField;
    [SerializeField] private TMP_InputField _registerEmailTextField;
    [SerializeField] private TMP_InputField _registerPasswordTextField;
    [SerializeField] private TMP_InputField _registerConfirmPasswordTextField;

    [Header("Listening to eventChannels")]
    [SerializeField] private String2EventChannelSO _loginEvent;
    [SerializeField] private String4EventChannelSO _registerEvent;


    public void Login()
    {
        _loginEvent.RaiseEvent(
            _loginEmailTextField.text,
            _loginPasswordTextField.text
        );
    }

    public void Register()
    {
        _registerEvent.RaiseEvent(
            _registerNameTextField.text,
            _registerEmailTextField.text,
            _registerPasswordTextField.text,
            _registerConfirmPasswordTextField.text
        );
    }
    
}
