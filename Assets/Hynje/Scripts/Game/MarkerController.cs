using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    [SerializeField] private GameObject lastPositionMarker;

    public void SetLastPositionMarker(bool onMarker)
    {
        lastPositionMarker.SetActive(onMarker);
    }
}
