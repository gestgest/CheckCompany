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

    [Header("Broadcasting on events")]
    [SerializeField] private String2EventChannelSO _debugLoginStatusEvent; //FirebaseAuthManager
    
    
    [Header("Listening to eventChannels")]
    [SerializeField] private String2EventChannelSO _loginEvent; //FirebaseAuthManager
    [SerializeField] private String4EventChannelSO _registerEvent;
    
    
    [SerializeField] private TextMeshProUGUI _debugText;

    public void Start()
    {
        _debugLoginStatusEvent._onEventRaised += DebugLoginEvent;
    }
    public void OnDestroy()
    {
        _debugLoginStatusEvent._onEventRaised -= DebugLoginEvent;
    }
    public void Login()
    {
        //what????
        if (_loginEvent._onEventRaised != null)
            _loginEvent.RaiseEvent(
                _loginEmailTextField.text,
                _loginPasswordTextField.text
            );
        else
        {
            _debugText.text = "이벤트가 없정";
        }
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

    public void DebugLoginEvent(string text, string a)
    {
        Debug.Log(text);
        _debugText.text = text;
    }
    
}
