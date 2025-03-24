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
    public void SetData(UserRecordData userRecordData, Sprite sprite, int index)
    {
        rankText.text       = $"{index + 1}";
        profileImage.sprite = sprite;
        gradeText.text      = $"{userRecordData.grade}급";
        nickNameText.text   = userRecordData.nickName;
        recordText.text     = $"{userRecordData.totalGameCount}전 {userRecordData.wins}승 {userRecordData.draws}무 {userRecordData.losses}패 {userRecordData.winRate}%";
        
        Index = index;
    }
}