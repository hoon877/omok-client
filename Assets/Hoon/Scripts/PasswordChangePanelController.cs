using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct changePassword
{
    public string password;
    public string changepassword;
}

public class PasswordChangePanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _changePasswordInputField;

    public void OnClickConfirmButton()
    {
        string password = _passwordInputField.text;
        string changePassword = _changePasswordInputField.text;
        
        if (string.IsNullOrEmpty(changePassword) || string.IsNullOrEmpty(password))
        {
            // TODO: 누락된 값 입렵 요청 팝업 표시
            return;
        }
        
        var passwordchangedata = new changePassword();
        
        passwordchangedata.password = password;
        passwordchangedata.changepassword = changePassword;
        
        StartCoroutine(NetworkManager.Instance.PasswordChange(passwordchangedata, () => {Debug.Log("비밀번호 변경 완료!"); Destroy(gameObject);}, () => {}));
    }

    public void OnClickBackButton()
    {
        Destroy(gameObject, 0.1f);
    }
}
