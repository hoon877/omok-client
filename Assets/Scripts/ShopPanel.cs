using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    GameObject shopPanel;
    Image shopPanelImage;

    void Start()
    {
        shopPanel = this.GameObject();
        shopPanelImage = this.GetComponent<Image>();
    }
}
