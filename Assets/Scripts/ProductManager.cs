using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductManager : MonoBehaviour
{
    public List<Product> products;
    public Transform shopPanel;
    public GameObject productsPrefab;
    
    
    
    void Start()
    {
        PopulateShop();
    }

    void PopulateShop()
    {
        foreach (var product in products)
        {
            GameObject newProduct = Instantiate(productsPrefab, shopPanel);
            newProduct.transform.Find("CoinAmount").GetComponent<Text>().text = product.coinAmount.ToString();
            newProduct.transform.Find("ProductPrice").GetComponent<Text>().text = product.ProductPrice.ToString();
            newProduct.transform.Find("ProductSprite").GetComponent<Image>().sprite = product.coinSprite;

            Button buyButton = newProduct.transform.Find("BuyButton").GetComponent<Button>();
            buyButton.onClick.AddListener(() => BuyProduct(product));
        }
    }
    
    /// <summary>
    /// TODO:코인 시스템 연동
    /// </summary>
    /// <param name="product"></param>
    void BuyProduct(Product product)
    {
        Debug.Log($"{product.coinAmount}코인 구매");
    }
    
    void Update()
    {
        
    }
}
