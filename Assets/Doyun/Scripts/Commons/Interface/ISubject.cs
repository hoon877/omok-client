using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubject
{
    /// <summary>
    /// 관찰자에게 상태변경 알림
    /// </summary>
    public void NotifyToObserver();

    /// <summary>
    /// 관찰자 등록
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IObserver observer);

    /// <summary>
    /// 관찰자 제거
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IObserver observer);
}
