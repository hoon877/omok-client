using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class RecordController : IDisposable
{
    private MonoBehaviour _coroutineRunner;
    private RecordUIPanelController _recordUIPanelController;
    private BoardController _boardController;
    private BoardClickHandler _boardClickHandler;
    
    private string _roomId;
    private List<GameMove> _gameMoves;
    private Stack<GameObject> _activeMarkerStack;
    private Stack<GameObject> _inactiveMarkerStack;
    
    public RecordController(string roomId)
    {
        _roomId = roomId;
        Debug.Log($"RecordController 초기화: roomId={roomId}");
        
        // 코루틴 실행을 위한 MonoBehaviour 찾기
        _coroutineRunner = Object.FindObjectOfType<MonoBehaviour>();
        
        if (_coroutineRunner != null)
        {
            _coroutineRunner.StartCoroutine(LoadGameRecord());
        }
        else
        {
            Debug.LogError("코루틴 실행을 위한 MonoBehaviour를 찾을 수 없습니다.");
        }
        
        _recordUIPanelController = Object.FindObjectOfType<RecordUIPanelController>();
        _boardController = Object.FindObjectOfType<BoardController>();
        _boardClickHandler = Object.FindObjectOfType<BoardClickHandler>();

        _recordUIPanelController.InitRecordUIController(this);
        _recordUIPanelController.onClickQuitButton = Dispose;
    }
    
    private IEnumerator LoadGameRecord()
    {
        Debug.Log($"게임 기록 로드 시작: {_roomId}");
        
        yield return _coroutineRunner.StartCoroutine(NetworkManager.Instance.GetGameRecord(
            _roomId,
            // 성공 콜백
            (moves) => {
                if (moves != null && moves.Count > 0)
                {
                    _gameMoves = moves;
                    Debug.Log($"게임 기록 로드 성공: {_gameMoves.Count}개의 수");
                    SetMarkerStack(_gameMoves);
                    // 처음 몇 개의 수 출력
                    int displayCount = Math.Min(5, _gameMoves.Count);
                    Debug.Log($"처음 {displayCount}개의 수:");
                    for (int i = 0; i < displayCount; i++)
                    {
                        var move = _gameMoves[i];
                        // 1차원 position을 row, col로 변환
                        int row = move.position / HYConstants.BoardSize;
                        int col = move.position % HYConstants.BoardSize;
                        Debug.Log($"수 #{i+1}: {move.player} at position={move.position} (row={row}, col={col}), 시간={move.timestamp}");
                    }
                    
                    if (_gameMoves.Count > displayCount)
                    {
                        Debug.Log("...");
                        // 마지막 수도 출력
                        var lastMove = _gameMoves[_gameMoves.Count - 1];
                        int lastRow = lastMove.position / HYConstants.BoardSize;
                        int lastCol = lastMove.position % HYConstants.BoardSize;
                        Debug.Log($"마지막 수 #{_gameMoves.Count}: {lastMove.player} at position={lastMove.position} (row={lastRow}, col={lastCol}), 시간={lastMove.timestamp}");
                    }
                }
                else
                {
                    Debug.LogWarning("게임 기록이 없거나 비어있습니다.");
                }
            },
            // 실패 콜백
            () => {
                Debug.LogError($"게임 기록 로드 실패: roomId={_roomId}");
            }
        ));
    }
    
    // 1차원 위치를 2차원 (row, col)로 변환하는 유틸리티 메서드
    private Vector2Int ConvertPositionToRowCol(int position)
    {
        int row = position / HYConstants.BoardSize;
        int col = position % HYConstants.BoardSize;
        
        Vector2Int gridPos = new Vector2Int(row, col);
        return gridPos;
    }

    private void SetMarkerStack(List<GameMove> moves)
    {
        _activeMarkerStack = new Stack<GameObject>();
        _inactiveMarkerStack = new Stack<GameObject>();
        
        for (int i = 0; i < moves.Count; i++)
        {
            var gridPos = ConvertPositionToRowCol(moves[i].position);
            var worldPos = _boardClickHandler.GetWorldPositionFromGrid(gridPos);
            var marker = moves[i].player == "X" ? HYConstants.MarkerType.Black : HYConstants.MarkerType.White;
            
            var markerObject = _boardController.GetMarker(marker, worldPos);
            _activeMarkerStack.Push(markerObject);
        }
    }

    public void PreviousStep()
    {
        if (_activeMarkerStack.Count <= 0) return;
        var temp = _activeMarkerStack.Pop();
        temp.GetComponent<MarkerController>().SetLastPositionMarker(false);
        temp.SetActive(false);
        if (_activeMarkerStack.Count > 0)
        {
            _activeMarkerStack.Peek().GetComponent<MarkerController>().SetLastPositionMarker(true);
        }
        _inactiveMarkerStack.Push(temp);
        
    }
    public void NextStep()
    {
        if (_inactiveMarkerStack.Count <= 0) return;
        var temp = _inactiveMarkerStack.Pop();
        if (_activeMarkerStack.Count > 0)
        {
            _activeMarkerStack.Peek().GetComponent<MarkerController>().SetLastPositionMarker(false);
        }
        temp.SetActive(true);
        temp.GetComponent<MarkerController>().SetLastPositionMarker(true);
        _activeMarkerStack.Push(temp);
    }

    public void ResetRecord()
    {
        while (_activeMarkerStack.Count > 0)
        {
            PreviousStep();
        }
    }

    public void EndRecord()
    {
        while (_inactiveMarkerStack.Count > 0)
        {
            NextStep();
        }
    }
    
    public void Dispose()
    {
        // 리소스 정리
        _gameMoves = null;
        _coroutineRunner.StopAllCoroutines();
        _recordUIPanelController.onClickQuitButton = null;
        Debug.Log("RecordController 리소스 정리 완료");
        HGameManager.Instance.EndGame();
    }
}