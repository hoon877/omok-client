using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserver
{
    /// <summary>
    /// 변경점 발생시 Subject에서 실행시킬 함수
    /// </summary>
    public void OnNotify();
}