using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameRecordCell : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text gameNumberText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button viewButton;

    // 셀 인덱스
    public int Index { get; private set; }
    
    // 셀 클릭 콜백
    public Action OnClick { get; set; }
    
    // 게임 ID (roomId)
    private string roomId;

    private void Awake()
    {
        viewButton.onClick.AddListener(() => OnClick?.Invoke());
    }

    public void SetData(GameSummary gameSummary, int index)
    {
        Index = index;
        roomId = gameSummary.roomId;

        // 게임 번호 (인덱스)
        var number = (index + 1).ToString();
        gameNumberText.text = number;

        // 승패 결과
        bool isWinner = IsCurrentUserWinner(gameSummary.winner, gameSummary.myPlayerType);
        resultText.text = isWinner ? "승리" : "패배";
        resultText.color = isWinner ? Color.green : Color.red;

        // 게임 날짜
        DateTime finishedDate = DateTime.Parse(gameSummary.finishedAt);
        dateText.text = finishedDate.ToString("MM.dd HH:ss");
    }

    private bool IsCurrentUserWinner(string winner, string myPlayerType)
    {
        // winner 값 형식에 따라 달라질 수 있음
        // 예: "BlackWin"이면 현재 플레이어가 흑돌이었는지 확인
        // 혹은 winner가 유저 ID일 수도 있음
        
        // TODO: 현재 로그인한 사용자와 winner 비교 로직 추가
        // 임시로 구현 (winner가 "BlackWin"이고 내가 흑돌이었는지 등 확인 필요)
        
        if (myPlayerType == "BlackPlayer" && winner == "BlackWin") return true;
        if (myPlayerType == "WhitePlayer" && winner == "WhiteWin") return true;
        
        return false;
    }
}
