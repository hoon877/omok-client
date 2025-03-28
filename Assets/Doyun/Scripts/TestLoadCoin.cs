using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadCoin : MonoBehaviour
{
    private void Start()
    {
        CoinManager.Instance.LoadCoin();
    }
}
