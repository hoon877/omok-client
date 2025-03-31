using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntantiateButton : MonoBehaviour
{
    [SerializeField] private GameObject instantiatePrefab;
    [SerializeField] private Transform canvasTranform;

    private GameObject instantiateObject;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickedRankButton);
    }

    public void OnClickedRankButton()
    {
        if (instantiateObject == null)
        {
            instantiateObject = Instantiate(instantiatePrefab, canvasTranform);
        }
        else
        {
            Debug.Log("랭크패널이 이미 열려 있음");
        }
    }
}
