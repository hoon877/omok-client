using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct SignupData
{
    public string username;
    public string nickname;
    public string password;
    public int profileIndex;
}

public class SignupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _confirmPasswordInputField;
    [SerializeField] private GameObject _profileImagePanel;
    [SerializeField] private Button _profileButton; // 프로필 버튼 (UI Button)
    [SerializeField] private Sprite _manProfileSprite; // 남자 프로필 이미지
    [SerializeField] private Sprite _womanProfileSprite; // 여자 프로필 이미지
    
    private static int _selectedProfileIndex = 0;
    private GameObject _profileSelectPanelObject; // 프로필 선택 패널 오브젝트 참조 변수
    private GameObject ForDestroy;
    
    
    public void OnClickConfirmButton()
    {
        var username = _emailInputField.text;
        var nickname = _nicknameInputField.text;
        var password = _passwordInputField.text;
        var confirmPassword = _confirmPasswordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            Debug.Log("입력 내용이 누락되었습니다.");
            return;
        }

        if (password.Equals(confirmPassword))
        {
            SignupData signupData = new SignupData
            {
                username = username,
                nickname = nickname,
                password = password,
                profileIndex = _selectedProfileIndex
            };
            Debug.Log(_selectedProfileIndex);
            Debug.Log($"📌 회원가입 데이터: {JsonUtility.ToJson(signupData)}");
            
            StartCoroutine(NetworkManager.Instance.Signup(signupData, () =>
            {
                Debug.Log("회원가입 완료!");
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
            Debug.Log("비밀번호가 서로 다릅니다.");
            _passwordInputField.text = "";
            _confirmPasswordInputField.text = "";
        }
    }

    public void OnClickProfileButton() // 프로필 변경 버튼 클릭 시
    {
        // 프로필 선택 패널이 이미 열려 있다면 다시 열지 않음
        if (_profileSelectPanelObject == null)
        {
            _profileSelectPanelObject = Instantiate(_profileImagePanel, transform);
            Debug.Log($"✅ 프로필 선택 패널 생성됨: {_profileSelectPanelObject.name}");
        }
        else
        {
            Debug.Log("프로필 선택 패널이 이미 열려 있음");
        }
    }

    public void OnClickManProfileButton() // 남자 프로필 선택
    {
        _selectedProfileIndex = 0;
        CloseProfilePanel();
    }
    
    public void OnClickWomanProfileButton() // 여자 프로필 선택
    {
        _selectedProfileIndex = 1;
        CloseProfilePanel();
    }

    private void CloseProfilePanel() 
    {
            ForDestroy = GameObject.Find("ProfileSelect Panel(Clone)");
            Destroy(ForDestroy, 0.1f);
    }

    private void Update()
    {
        if (_selectedProfileIndex == 0)
        {
            _profileButton.image.sprite = _manProfileSprite;//남자 프로필 변경
        }
        else
        {
            _profileButton.image.sprite = _womanProfileSprite; //여자 프로필 변경
        }
    }

    public void OnClickBackButton()
    {
        ForDestroy = GameObject.Find("Signup Panel(Clone)");
        Destroy(ForDestroy, 0.1f);
    }
}
