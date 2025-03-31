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
    private bool isMouseOverBuyButton;

    private void Awake()
    {
        normalColor = ProdBgImage.color;
        altColor = new Color32(255,178,103,255);
        isMouseOverBgImage = false;
        isMouseOverBuyButton = false;
    }

    public void IsMouseOverBgImage()
    {
        isMouseOverBgImage = !isMouseOverBgImage;
        ChangeColor();
    }
    
    public void IsMouseOverBuyButton()
    {
        isMouseOverBuyButton = !isMouseOverBuyButton;
        ChangeColor();
    }
    
    private void ChangeColor()
    {
        ProdBgImage.color = isMouseOverBgImage || isMouseOverBuyButton ? altColor : normalColor;
    }
    
    

}