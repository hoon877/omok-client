using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _profileImagePanel;
    [SerializeField] private GameObject _passwordChangePanel;
    private GameObject _profileSelectPanelObject;
    private static int _selectedProfileIndex = 0;
    private GameObject ForDestroy;
    
    
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
        ProfileResult profileData = new ProfileResult { profileIndex = _selectedProfileIndex };
        StartCoroutine(NetworkManager.Instance.SaveProfile(profileData, () => { 
            Debug.Log("프로필 변경 완료!");}, () => {}));
        CloseProfilePanel();
    }
    
    public void OnClickWomanProfileButton() // 여자 프로필 선택
    {
        _selectedProfileIndex = 1;
        ProfileResult profileData = new ProfileResult { profileIndex = _selectedProfileIndex };
        StartCoroutine(NetworkManager.Instance.SaveProfile(profileData, () => { 
            Debug.Log("프로필 변경 완료!");}, () => {}));
        CloseProfilePanel();
    }
    
    private void CloseProfilePanel() 
    {
        ForDestroy = GameObject.Find("SettingProfileSelect Panel(Clone)");
        Destroy(ForDestroy, 0.1f);
    }

    public void OnClickPasswordChangeButton()
    {
        Instantiate(_passwordChangePanel, transform);
    }

    public void OnClickLogoutButton()
    {
        StartCoroutine(NetworkManager.Instance.Signout(OnLogoutSuccess, OnLogoutFailure));
    }
    
    void OnLogoutSuccess()
    {
        Debug.Log("메인 화면으로 이동");
        _mainPanel.SetActive(false);
        gameObject.SetActive(false);
        _loginPanel.SetActive(true);
    }

    void OnLogoutFailure()
    {
        Debug.Log("로그아웃 실패");
    }

    public void OnClickBackButton()
    {
        _mainPanel.SetActive(true);
        Invoke(nameof(DeactivateSettingPanel), 0.1f);
    }
    
    private void DeactivateSettingPanel()
    {
        _settingPanel.SetActive(false);
    }
}
