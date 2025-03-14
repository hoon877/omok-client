using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public struct SignupData
{
    public string username;
    public string nickname;
    public string password;
    public int profileImage;
}

public class SignupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _confirmPasswordInputField;
    public IEnumerator Signup(SignupData signupData, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(signupData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signup", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);

                if (www.responseCode == 409)
                {
                    // TODO: 중복 사용자 생성 팝업 표시
                    Debug.Log("중복사용자");
                    // GameManager.Instance.OpenConfirmPanel("이미 존재하는 사용자입니다.", () =>
                    // {
                    //     failure?.Invoke();
                    // });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                Debug.Log("Result: " + result);
                
                // 회원가입 성공 팝업 표시
                // GameManager.Instance.OpenConfirmPanel("회원 가입이 완료 되었습니다.", () =>
                // {
                //     success?.Invoke();
                // });
            }
        }
    }
    
    public void OnClickConfirmButton()
    {
        var username = _emailInputField.text;
        var nickname = _nicknameInputField.text;
        var password = _passwordInputField.text;
        var confirmPassword = _confirmPasswordInputField.text;
        

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            // GameManager.Instance.OpenConfirmPanel("입력 내용이 누락되었습니다.", () =>
            // {
            //     
            // });
            // return;
        }

        if (password.Equals(confirmPassword))
        {
            SignupData signupData = new SignupData();
            signupData.username = username;
            signupData.nickname = nickname;
            signupData.password = password;
            
            // 서버로 SignupData 전달하면서 회원가입 진행
            StartCoroutine(NetworkManager.Instance.Signup(signupData, () =>
            {
                Destroy(gameObject);
            }, () =>
            {
                _emailInputField.text = "";
                _nicknameInputField.text = "";
                _passwordInputField.text = "";
                _confirmPasswordInputField.text = "";
            }));
        }
        else
        {
            // GameManager.Instance.OpenConfirmPanel("비밀번호가 서로 다릅니다.", () =>
            // {
            //     _passwordInputField.text = "";
            //     _confirmPasswordInputField.text = "";
            // });
        }
    }
    
}
