using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Experimental.GlobalIllumination;

struct CoinResponse
{
    public string id;
    public string username;
    public string nickname;
    public int coin;
}

public class CoinManager : HSingleton<CoinManager>, ISubject
{
    // PS. Edit > ProjectSetting > Script Execution Order 에서 스크립트 우선순위를 조정하였음
    // 추후 싱글톤 스크립트로 우선순위 조정

    string path = "http://localhost:3000/users/";

    [SerializeField] private int m_coins;

    [SerializeField] private string testUserName;
    [SerializeField] private string testPassword;

    /// <summary>
    /// 코인개수 변화시 옵저버들에게 알리고 코인저장
    /// </summary>
    public int CoinsCount
    {
        get { return m_coins; }
        set
        {
            m_coins = value;
            NotifyToObserver();
            SaveCoin(m_coins);
        }
    }

    // 코인갯수를 확인하는 옵저버들
    public List<IObserver> observers;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void Start()
    {
        observers = new List<IObserver>();
        TestLogin();
        // LoadCoin();
    }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// 테스트용으로 로그인하는 함수
    /// </summary>
    private void TestLogin()
    {
        StartCoroutine(TestLoginCoroutine(LoadCoin));
    }

    IEnumerator TestLoginCoroutine(Action success)
    {
        // 임시 로그인 정보
        LoginData loginData = new LoginData
        { username = testUserName, password = testPassword };

        string jsonString = JsonUtility.ToJson(loginData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(HConstants.ServerURL + "/users/signin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            { }
            else
            {
                var cookie = www.GetResponseHeader("set-cookie");
                if (!string.IsNullOrEmpty(cookie))
                {
                    int lastIndex = cookie.LastIndexOf(";");
                    string sid = cookie.Substring(0, lastIndex);
                    PlayerPrefs.SetString("sid", sid);
                }

                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<LoginResult>(resultString);

                if (result.result == 0)
                {
                    Debug.Log("실패");
                    // 유저네임 유효하지 않음
                }
                else if (result.result == 1)
                {
                    Debug.Log("실패");
                    // 패스워드가 유효하지 않음
                }
                else if (result.result == 2)
                {
                    Debug.Log("성공");
                    success?.Invoke();
                    // 성공

                }
            }
        }
    }

    #region 코인 저장

    void SaveCoin(int value)
    {
        StartCoroutine(SaveCoinCoroutine(CoinsCount));
    }

    IEnumerator SaveCoinCoroutine(int value)
    {
        string url = path + "/savecoin";

        WWWForm form = new WWWForm();
        form.AddField("coin", value);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            // 오류 발생시
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("저장 실패 : " + www.downloadHandler.text);
            }
            // 데이터베이스 접속 성공시
            else
            {
                Debug.Log("저장 완료");
            }
        }
    }
    #endregion

    #region 코인 불러오기
    public void LoadCoin()
    {
        StartCoroutine(LoadCoinCoroutine());
    }

    IEnumerator LoadCoinCoroutine()
    {
        string url = path + "/coin";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            // 오류 발생시
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("코인 불러오기 오류 발생 : " + www.downloadHandler.text);
            }
            // 데이터베이스 접속 성공시
            else
            {
                Debug.Log("코인 불러오기 접속 성공 : " + www.downloadHandler.text);

                string responseText = www.downloadHandler.text;
                CoinResponse coinRespons = JsonUtility.FromJson<CoinResponse>(responseText);
                CoinsCount = coinRespons.coin;
            }
        }
    }
    #endregion

    #region 코인 더하기
    public void AddCoin(int amount)
    {
        CoinsCount += amount;
    }

    public void RemoveCoin(int amount)
    {
        CoinsCount -= amount;
    }
    #endregion

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 옵저버

    /// <summary>
    /// 관찰자 추가
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    /// <summary>
    /// 관찰자 제거
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    /// <summary>
    /// 코인개수 표시같은 변화에 반응하는 클래스에 사용할 함수
    /// </summary>
    public void NotifyToObserver()
    {
        if (observers.Count > 0)
        {
            foreach (IObserver observer in observers)
            {
                observer.OnNotify();
            }
        }
    }

    #endregion

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ??= 로 null인지 판별후 Clear()
        (observers ??= new List<IObserver>()).Clear();
    }
}