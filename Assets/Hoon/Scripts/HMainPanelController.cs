using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct ProfileResult
{
    public int profileIndex;
}

public struct Coin
{
    public string username;
    public string nickname;
    public int coin;
}

public class HMainPanelController : MonoBehaviour
{
    [SerializeField] private Button _profileButton; // 프로필 버튼 (UI Button)
    [SerializeField] private Sprite _manProfileSprite; // 남자 프로필 이미지
    [SerializeField] private Sprite _womanProfileSprite; // 여자 프로필 이미지
    [SerializeField] private TMP_Text _coinText;
    [SerializeField] private TMP_Text _profileText;
    [SerializeField] private GameObject _gameTypePanel;
    [SerializeField] private GameObject _gameRecordPanel;
    [SerializeField] private GameObject _settingPanel;

    private int _selectedProfileIndex;
    private int _coinCount;
    private void Awake()
    {
        StartCoroutine(NetworkManager.Instance.GetScore(ProfileResult =>
        {
            _selectedProfileIndex = ProfileResult.profileIndex;
        }, () =>
        {
            Debug.Log("로그인이 필요합니다.");
        }));

        StartCoroutine(NetworkManager.Instance.GetCoin(coin =>
        {
            _coinCount = coin.coin;
            _coinText.text = _coinCount.ToString();
            _profileText.text = coin.nickname;
        }, () =>
        {
            Debug.Log("로그인이 필요합니다.");
        }));
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

    public void OnClickGamestartButton()
    {
        Instantiate(_gameTypePanel, transform).GetComponent<PanelController>().Show();
    }

    public void OnClickReplayButton()
    {
        Instantiate(_gameRecordPanel, transform);
    }

    public void OnClickRankButton()
    {
        SceneManager.LoadScene("Ranking Board Cell Scene");
    }

    public void OnClickStoreButton()
    {
        SceneManager.LoadScene("Doyun/Scenes/DoyunTestScene");
    }

    public void OnClickSettingButton()
    {
        _settingPanel.SetActive(true);
    }
}
