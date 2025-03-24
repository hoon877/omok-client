using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICallBack
{
    public Action TriggerAction { get; set; }

    public void Subscribe(Action action);

    public void Unsubscribe();

    public void TriggerEvent();
}
