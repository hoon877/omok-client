using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject blackMarkerPrefab;
    [SerializeField] private GameObject whiteMarkerPrefab;
    [SerializeField] private GameObject forbiddenMarkerPrefab;

    private MarkerController _lastMarker;
    
    public void SetMarker(Constants.MarkerType markerType, Vector3 position)
    {
        _lastMarker?.SetLastPositionMarker(false);
        
        var markerPrefab = (markerType == Constants.MarkerType.Black) ? blackMarkerPrefab : whiteMarkerPrefab;
        var markerObject = Instantiate(markerPrefab, position, Quaternion.identity);
        var markerController = markerObject.GetComponent<MarkerController>();
        
        _lastMarker = markerController;
        
        _lastMarker.SetLastPositionMarker(true);
    }

    public void SetForbiddenMarker(Vector3 position)
    {
        Instantiate(forbiddenMarkerPrefab, position, Quaternion.identity);
    }

    public void HideForbiddenMarkers()
    {
        
    }
}