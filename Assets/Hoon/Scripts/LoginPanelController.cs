using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    private Canvas _canvas;
    
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
            // Destroy(gameObject);
            gameObject.SetActive(false);
            _mainPanel.SetActive(true);
            CoinManager.Instance.LoadCoin();
            
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
