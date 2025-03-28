using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductManager : MonoBehaviour
{
    public List<Product> products;
    public Transform targetPanel;
    public GameObject productPrefab;
    
    public Sprite coin1KImage;
    
    void Start()
    {
        PopulateShop();
    }
    
    private void PopulateShop()
    {
        products = new List<Product>
        {
            new Product(1000, 1000, coin1KImage),
            new Product(3000, 2500, coin1KImage),
            new Product(3000, 8000, coin1KImage)
        };
        
        foreach (var product in products)
        {
            // 상품 프리팹 생성
            GameObject newProduct = Instantiate(productPrefab, targetPanel);

            // ProductPrefab 스크립트 가져오기
            ProductPrefab productComponent = newProduct.GetComponent<ProductPrefab>();
            if (productComponent != null)
            {
                // 상품 데이터 초기화
                productComponent.Setup(product, () => BuyProduct(product));
            }
            else
            {
                Debug.LogError("ProductPrefab 컴포넌트를 찾을 수 없습니다.");
            }
        }
        
    }
    
    void BuyProduct(Product product)
    {
        Debug.Log($"{product.coinAmount} 코인 구매 (가격: {product.coinPrice}krw)");
        // TODO: 코인 시스템 연동
    }
}
