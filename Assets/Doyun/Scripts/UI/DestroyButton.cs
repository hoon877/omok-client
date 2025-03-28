using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestroyButton : MonoBehaviour
{
    [SerializeField] private GameObject destroyObject;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickedDestroyButton);
    }

    private void OnClickedDestroyButton()
    {
        Destroy(destroyObject);
    }
}
