using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinManager : HSingleton<CoinManager>, ISubject
{
    // PS. Edit > ProjectSetting > Script Execution Order 에서 스크립트 우선순위를 조정하였음
    // 추후 싱글톤 스크립트로 우선순위 조정

    /// <summary>
    /// 코인 데이터 경로, 
    /// </summary>
    private const string m_coinPath = "CoinData";
    [SerializeField] private int m_coins;

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

    [SerializeField] private bool resetCoin;

    // 코인갯수를 확인하는 옵저버들
    public List<IObserver> observers;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void Start()
    {
        observers = new List<IObserver>();
        ResetCoin(resetCoin);
        LoadCoin();
    }
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 코인 추가/감소
    /// <summary>
    /// value만큼 코인 추가
    /// </summary>
    /// <param name="value"></param>
    public void AddCoin(int value)
    {
        CoinsCount += value;
    }

    /// <summary>
    /// value만큼 코인 차감
    /// </summary>
    /// <param name="value"></param>
    public void RemoveCoin(int value)
    {
        CoinsCount -= value;
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

    // 현재는 PlayerPrefs으로 하고있음, 추후 서버로 변경
    #region 서버(플레이어 프리팹)관련
    /// <summary>
    /// 서버 혹은 플레이어 프리팹의 코인불러오기
    /// </summary>
    /// <param name="value"></param>
    public void LoadCoin()
    {
        CoinsCount = PlayerPrefs.GetInt(m_coinPath, 0);
    }

    /// <summary>
    /// 서버 혹은 플레이어 프리팹에 현재 코인저장
    /// </summary>
    /// <param name="value"></param>
    public void SaveCoin(int value)
    {
        PlayerPrefs.SetInt(m_coinPath, value);
    }

    private void ResetCoin(bool resetCoin)
    {
        if (resetCoin)
            PlayerPrefs.DeleteKey(m_coinPath);
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ??= 로 null인지 판별후 Clear()
        (observers ??= new List<IObserver>()).Clear();
    }
}