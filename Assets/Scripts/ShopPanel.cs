using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    public static ShopPanel Instance { get; private set; }

    public GameObject shopPanel;
    public Image shopPanelImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        shopPanel = this.gameObject;
        shopPanelImage = GetComponent<Image>();

        if (shopPanelImage == null)
        {
            Debug.LogError("ShopPanel에 Image 컴포넌트가 없습니다!");
        }
    }
}