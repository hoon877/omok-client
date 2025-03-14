using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubject
{
    public void NotifyToObserver();

    public void AddObserver(IObserver observer);

    public void RemoveObserver(IObserver observer);
}
