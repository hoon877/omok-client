using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductMouseOver : MonoBehaviour
{
    public Image ProdBgImage;
    public GameObject buyButton;
    public Color normalColor;
    public Color32 altColor;
    
    private bool isMouseOverBgImage;
    public bool isMouseOverBuyButton;

    private void Awake()
    {
        normalColor = ProdBgImage.color;
        altColor = new Color32(255,178,103,255);
    }

    public void IsMouseOverBgImage()
    {
        isMouseOverBgImage = !isMouseOverBgImage;
        ChangeColor();
    }
    
    public void IsMouseOverBuyButton()
    {
        isMouseOverBuyButton = !isMouseOverBuyButton;
    }
    
    private void ChangeColor()
    {
        ProdBgImage.color = isMouseOverBgImage || isMouseOverBuyButton ? altColor : normalColor;
    }
    
    

}
