using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICallBack
{
    /// <summary>
    /// 특정 함수를 실행시킬 델리게이트
    /// </summary>
    public Action TriggerAction { get; set; }

    /// <summary>
    /// 실행시킬 함수를 구독
    /// </summary>
    /// <param name="action"></param>
    public void Subscribe(Action action);

    /// <summary>
    /// 구독한 함수를 제거
    /// </summary>
    public void Unsubscribe();

    /// <summary>
    /// 함수 종료시 호출 델리게이트실행
    /// </summary>
    public void TriggerEvent();
}
