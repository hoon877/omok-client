using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct LoginData
{
    public string username;
    public string password;
}

public struct LoginResult
{
    public int result;
}


public class LoginPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _signupPanel;
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    
    public void OnClickLogin()
    {
        string username = _usernameInputField.text;
        string password = _passwordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // TODO: 누락된 값 입렵 요청 팝업 표시
            return;
        }

        var signinData = new LoginData();
        signinData.username = username;
        signinData.password = password;
        
        StartCoroutine(NetworkManager.Instance.Signin(signinData, () =>
        {
            Debug.Log("성공");
            Destroy(gameObject);
        }, result =>
        {
            if (result == 0)
            {
                _usernameInputField.text = "";
            }
            else if (result == 1)
            {
                _passwordInputField.text = "";
            }
        }));
    }

    public void OnClickSignUp()
    {
        Instantiate(_signupPanel, transform);
    }
}
