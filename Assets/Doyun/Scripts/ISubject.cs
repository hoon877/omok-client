using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubject
{
    public void NotifyToObserver();

    public void AddObservers(IObserver observer);

    public void RemoveObservers(IObserver observer);
}
