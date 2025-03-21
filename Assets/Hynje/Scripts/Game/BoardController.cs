using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject blackMarkerPrefab;
    [SerializeField] private GameObject whiteMarkerPrefab;
    [SerializeField] private Transform markersParent;
    
    private MarkerController _lastMarker;
    private List<GameObject> _forbiddenMarkers = new List<GameObject>();
    
    public void SetMarker(Constants.MarkerType markerType, Vector3 position)
    {
        _lastMarker?.SetLastPositionMarker(false);
        
        var markerPrefab = (markerType == Constants.MarkerType.Black) ? blackMarkerPrefab : whiteMarkerPrefab;
        var markerObject = Instantiate(markerPrefab, position, Quaternion.identity);
        markerObject.transform.SetParent(markersParent);
        var markerController = markerObject.GetComponent<MarkerController>();
        
        _lastMarker = markerController;
        
        _lastMarker.SetLastPositionMarker(true);
    }

    public void SetForbiddenMarker(Vector3 position)
    {
        var forbiddenMarkerObject = ObjectPool.Instance.GetObject();
        forbiddenMarkerObject.transform.position = position;
        _forbiddenMarkers.Add(forbiddenMarkerObject);
    }

    public void HideForbiddenMarkers()
    {
        foreach (var forbiddenMarker in _forbiddenMarkers)
        {
            ObjectPool.Instance.ReturnObject(forbiddenMarker);
        }
        _forbiddenMarkers.Clear();
    }
}