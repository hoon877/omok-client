using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
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
    private string m_coinPath = "CoinData";
    private int m_coins;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
    private void Start()
    {
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
        m_coins += value;
        SaveCoin(m_coins);
    }

    /// <summary>
    /// value만큼 코인 차감
    /// </summary>
    /// <param name="value"></param>
    public void RemoveCoin(int value)
    {
        m_coins -= value;
        SaveCoin(m_coins);
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

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
