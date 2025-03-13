using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour, ISubject
{
    #region 싱글톤(추후 Singleton 클래스 상속받기)
    static CoinManager instance;
    public static CoinManager Instance {  get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// 코인 데이터 경로, 
    /// </summary>
    private const string m_coinPath = "CoinData";
    private int m_coins;

    public List<IObserver> observers;
    public event Action CoinEvent;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
    private void Start()
    {
        LoadCoin();
        NotifyToObserver();
    }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 코인 추가/감소
    /// <summary>
    /// value만큼 코인 추가
    /// </summary>
    /// <param name="value"></param>
    public void AddCoin(int value)
    {
        m_coins += value;

        NotifyToObserver();
    }

    /// <summary>
    /// value만큼 코인 차감
    /// </summary>
    /// <param name="value"></param>
    public void RemoveCoin(int value)
    {
        m_coins -= value;

        NotifyToObserver();
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 옵저버
    
    public void AddObservers(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObservers(IObserver observer)
    {
        observers.Remove(observer);
    }

    /// <summary>
    /// 코인개수 표시같은 변화에 반응하는 클래스에 사용할 함수
    /// </summary>
    public void NotifyToObserver()
    {
        foreach (IObserver observer in observers)
        {
            observer.OnNotify();
        }

        SaveCoin(m_coins);
    }

    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region Subscribe 패턴

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
        m_coins = PlayerPrefs.GetInt(m_coinPath, 0);
    }

    /// <summary>
    /// 서버 혹은 플레이어 프리팹에 현재 코인저장
    /// </summary>
    /// <param name="value"></param>
    public void SaveCoin(int value)
    {
        PlayerPrefs.SetInt(m_coinPath, value);
    }
    #endregion
}