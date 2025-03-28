using UnityEngine;
using UnityEngine.UI;

public class Product
{
    public int coinAmount;         // 코인 수량
    public int coinPrice;       // 가격
    public Sprite coinSprite;      // 스프라이트

    public Product(int coinAmount, int coinPrice, Sprite coinSprite)
    {
        this.coinAmount = coinAmount;
        this.coinPrice = coinPrice;
        this.coinSprite = coinSprite;
    }
}