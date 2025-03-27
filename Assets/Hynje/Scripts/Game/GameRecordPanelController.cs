using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameRecordPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button replayButton;
    
    [Header("Game Board")]
    [SerializeField] private Transform boardContainer;
    [SerializeField] private GameObject blackStonePrefab;
    [SerializeField] private GameObject whiteStonePrefab;
    [SerializeField] private GameObject boardPrefab;
    
    private string currentRoomId;
    private List<GameMove> gameRecordMoves;
    private GameObject boardObject;
    private List<GameObject> stoneObjects = new List<GameObject>();
    
    private void Awake()
    {
        closeButton.onClick.AddListener(ClosePanel);
        replayButton.onClick.AddListener(StartReplay);
        panel.SetActive(false);
    }
    
    public void ShowGameRecord(string roomId)
    {
        currentRoomId = roomId;
        panel.SetActive(true);
        
        // 기본 정보 초기화
        titleText.text = "게임 기록 로드 중...";
        dateText.text = "";
        resultText.text = "";
        durationText.text = "";
        
        // 기존 보드와 돌 제거
        ClearBoard();
        
        // 게임 기록 로드
        StartCoroutine(LoadGameRecord(roomId));
    }
    
    private IEnumerator LoadGameRecord(string roomId)
    {
        yield return StartCoroutine(NetworkManager.Instance.GetGameRecord(
            roomId,
            // 성공 콜백
            (moves) => {
                gameRecordMoves = moves;
                DisplayGameInfo(roomId);
                SetupBoard();
            },
            // 실패 콜백
            () => {
                Debug.LogError("게임 기록 상세 정보 로드 실패");
                titleText.text = "게임 기록을 불러오지 못했습니다";
            }
        ));
    }
    
    private void DisplayGameInfo(string roomId)
    {
        if (gameRecordMoves == null || gameRecordMoves.Count == 0)
        {
            titleText.text = "기록 없음";
            return;
        }
        
        titleText.text = $"게임 기록: {roomId.Substring(0, 8)}";
        
        // 날짜와 시간
        DateTime firstMoveTime = DateTime.Parse(gameRecordMoves[0].timestamp);
        DateTime lastMoveTime = DateTime.Parse(gameRecordMoves[gameRecordMoves.Count - 1].timestamp);
        
        dateText.text = $"날짜: {firstMoveTime:yyyy.MM.dd}";
        
        // 게임 시간
        TimeSpan duration = lastMoveTime - firstMoveTime;
        durationText.text = $"게임 시간: {FormatDuration(duration)}";
        
        // 승패 결과 (마지막 돌의 플레이어 또는 다른 방식으로 결정)
        string winner = DetermineWinner();
        bool isWinner = IsCurrentUserWinner(winner);
        resultText.text = $"결과: {(isWinner ? "승리" : "패배")}";
        resultText.color = isWinner ? Color.green : Color.red;
    }
    
    private string DetermineWinner()
    {
        // 게임 기록에서 승자 결정
        // 예: 마지막 돌을 놓은 플레이어가 승자일 수 있음
        if (gameRecordMoves.Count > 0)
        {
            return gameRecordMoves[gameRecordMoves.Count - 1].player;
        }
        return "";
    }
    
    private bool IsCurrentUserWinner(string winner)
    {
        // 실제 구현에 맞게 수정 필요
        string myRole = PlayerPrefs.GetString("LastGameRole", "");
        
        if (myRole == "Black" && winner == "X") return true;
        if (myRole == "White" && winner == "O") return true;
        
        return false;
    }
    
    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{duration.Hours}시간 {duration.Minutes}분";
        }
        else if (duration.TotalMinutes >= 1)
        {
            return $"{duration.Minutes}분 {duration.Seconds}초";
        }
        else
        {
            return $"{duration.Seconds}초";
        }
    }
    
    private void SetupBoard()
    {
        // 기존 보드 제거
        ClearBoard();
        
        // 새 보드 생성
        boardObject = Instantiate(boardPrefab, boardContainer);
        
        // DisplayAllStones 또는 StartReplay 선택
        DisplayAllStones();
    }
    
    private void DisplayAllStones()
    {
        if (gameRecordMoves == null) return;
        
        for (int i = 0; i < gameRecordMoves.Count; i++)
        {
            GameMove move = gameRecordMoves[i];
            PlaceStone(move.position, move.player, i);
        }
    }
    
    private void PlaceStone(int position, string player, int moveIndex = -1)
    {
        int boardSize = HYConstants.BoardSize;
        
        // 1D 위치를 2D 좌표로 변환
        int x = position % boardSize;
        int y = position / boardSize;
        
        // 좌표를 Unity 위치로 변환 (15x15 보드 기준, 적절히 조정 필요)
        float cellSize = 1.0f; // 적절한 크기로 조정
        Vector3 worldPos = new Vector3(
            x * cellSize - (boardSize / 2) * cellSize + cellSize / 2,
            0.1f, // 약간 위로 올림
            y * cellSize - (boardSize / 2) * cellSize + cellSize / 2
        );
        
        // 흑돌 또는 백돌 결정
        GameObject stonePrefab = (player == "X") ? blackStonePrefab : whiteStonePrefab;
        GameObject stone = Instantiate(stonePrefab, worldPos, Quaternion.identity, boardObject.transform);
        
        // 움직임 순서 표시 (옵션)
        if (moveIndex >= 0)
        {
            // 텍스트 컴포넌트가 있다면 움직임 번호 표시
            TextMeshProUGUI textComp = stone.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = (moveIndex + 1).ToString();
            }
        }
        
        stoneObjects.Add(stone);
    }
    
    private IEnumerator ReplayCoroutine()
    {
        if (gameRecordMoves == null || gameRecordMoves.Count == 0) yield break;
        
        // 기존 돌 제거
        foreach (var stone in stoneObjects)
        {
            Destroy(stone);
        }
        stoneObjects.Clear();
        
        // 돌을 하나씩 놓기
        for (int i = 0; i < gameRecordMoves.Count; i++)
        {
            GameMove move = gameRecordMoves[i];
            PlaceStone(move.position, move.player, i);
            
            // 다음 돌을 놓기 전에 잠시 대기
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void StartReplay()
    {
        StopAllCoroutines();
        StartCoroutine(ReplayCoroutine());
    }
    
    private void ClearBoard()
    {
        foreach (var stone in stoneObjects)
        {
            Destroy(stone);
        }
        stoneObjects.Clear();
        
        if (boardObject != null)
        {
            Destroy(boardObject);
            boardObject = null;
        }
    }
    
    private void ClosePanel()
    {
        StopAllCoroutines();
        panel.SetActive(false);
        ClearBoard();
    }
}
