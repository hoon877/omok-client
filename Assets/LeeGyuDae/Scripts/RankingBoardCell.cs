using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingBoardCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Image           profileImage;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI nickNameText;
    [SerializeField] private TextMeshProUGUI recordText;

    public int Index { get; private set; }
    public void SetData(UserRankingData userRankingData, Sprite sprite, int index)
    {
        Index = index;
        
        rankText.text = $"{userRankingData.Rank + 1}";
        profileImage.sprite = sprite;
        gradeText.text = $"{userRankingData.Grade}급";
        nickNameText.text = userRankingData.NickName;
        
        float winRate = userRankingData.TotalGameCount > 0 ? (userRankingData.Wins / (float)userRankingData.TotalGameCount) * 100 : 0;
        recordText.text = $"{userRankingData.TotalGameCount}전 {userRankingData.Wins}승 {userRankingData.Losses}패 {winRate:F1}%";
    }
}