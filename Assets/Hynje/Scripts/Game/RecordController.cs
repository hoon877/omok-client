using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordController : IDisposable
{
    private string roomId;
    private List<GameMove> gameMoves;
    private MonoBehaviour coroutineRunner;
    private int boardSize = 15; // 오목판 크기, 필요에 따라 조정
    
    public RecordController(string roomId)
    {
        this.roomId = roomId;
        Debug.Log($"RecordController 초기화: roomId={roomId}");
        
        // 코루틴 실행을 위한 MonoBehaviour 찾기
        coroutineRunner = GameObject.FindObjectOfType<MonoBehaviour>();
        
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(LoadGameRecord());
        }
        else
        {
            Debug.LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다.");
        }
    }
    
    private IEnumerator LoadGameRecord()
    {
        Debug.Log($"게임 기록 로드 시작: {roomId}");
        
        yield return coroutineRunner.StartCoroutine(NetworkManager.Instance.GetGameRecord(
            roomId,
            // 성공 콜백
            (moves) => {
                if (moves != null && moves.Count > 0)
                {
                    gameMoves = moves;
                    Debug.Log($"게임 기록 로드 성공: {gameMoves.Count}개의 수");
                    
                    // 처음 몇 개의 수 출력
                    int displayCount = Math.Min(5, gameMoves.Count);
                    Debug.Log($"처음 {displayCount}개의 수:");
                    for (int i = 0; i < displayCount; i++)
                    {
                        var move = gameMoves[i];
                        // 1차원 position을 row, col로 변환
                        int row = move.position / boardSize;
                        int col = move.position % boardSize;
                        Debug.Log($"수 #{i+1}: {move.player} at position={move.position} (row={row}, col={col}), 시간={move.timestamp}");
                    }
                    
                    if (gameMoves.Count > displayCount)
                    {
                        Debug.Log("...");
                        // 마지막 수도 출력
                        var lastMove = gameMoves[gameMoves.Count - 1];
                        int lastRow = lastMove.position / boardSize;
                        int lastCol = lastMove.position % boardSize;
                        Debug.Log($"마지막 수 #{gameMoves.Count}: {lastMove.player} at position={lastMove.position} (row={lastRow}, col={lastCol}), 시간={lastMove.timestamp}");
                    }
                }
                else
                {
                    Debug.LogWarning("게임 기록이 없거나 비어있습니다.");
                }
            },
            // 실패 콜백
            () => {
                Debug.LogError($"게임 기록 로드 실패: roomId={roomId}");
            }
        ));
    }
    
    // 1차원 위치를 2차원 (row, col)로 변환하는 유틸리티 메서드
    private (int row, int col) ConvertPositionToRowCol(int position)
    {
        int row = position / boardSize;
        int col = position % boardSize;
        return (row, col);
    }
    
    public void Dispose()
    {
        // 리소스 정리
        gameMoves = null;
        Debug.Log("RecordController 리소스 정리 완료");
    }
}