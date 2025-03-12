using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private GameObject blackMarkerPrefab;
    [SerializeField] private GameObject whiteMarkerPrefab;
    [SerializeField] private GameObject lastPositionMarker;
    private void Start()
    {
        lastPositionMarker.SetActive(false);
    }
    
    public void SetMarker(Constants.MarkerType markerType, Vector3 position)
    {
        var markerPrefab = (markerType == Constants.MarkerType.Black) ? blackMarkerPrefab : whiteMarkerPrefab;
        Instantiate(markerPrefab, position, Quaternion.identity);
        SetLastPositionMarker(position);
    }

    private void SetLastPositionMarker(Vector3 position)
    {
        lastPositionMarker.SetActive(true);
        lastPositionMarker.transform.position = position;
    }
}