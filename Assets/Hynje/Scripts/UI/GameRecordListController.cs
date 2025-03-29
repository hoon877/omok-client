using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRecordListController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject scrollRectGameObject;
    [SerializeField] private GameObject gameCellPrefab; // 게임 기록 셀 프리팹
    //[SerializeField] private ObjectPool gameCellPool;

    [Header("Cell Settings")]
    [SerializeField] private float cellHeight = 150f;
    [SerializeField] private float cellSpacing = 20f;
    [SerializeField] private int cellBuffer = 2;

    // Scroll Rect
    private ScrollRect scrollRect;
    private RectTransform scrollRectRectTransform;
    private float prevScrollRectYValue = 1f;

    // Game Record Data
    private List<GameSummary> gameRecordList = new List<GameSummary>();
    private LinkedList<GameRecordCell> visibleCellList;

    // Cell
    private float cellSize;

    // 게임 기록 선택 콜백
    public Action<string> OnGameRecordSelected;

    private void Awake()
    {
        scrollRect = scrollRectGameObject.GetComponent<ScrollRect>();
        scrollRectRectTransform = scrollRectGameObject.GetComponent<RectTransform>();

        scrollRect.onValueChanged.AddListener(OnValueChanged);
    }

    private void Start()
    {
        LoadGameRecords();
        //LoadGameRecordsDebug();
    }
    
    /// <summary>
    /// 디버깅용 - 셀 생성 없이 게임 기록 데이터만 로드
    /// </summary>
    private void LoadGameRecordsDebug()
    {
        Debug.Log("게임 기록 로드 시작 (디버그 모드)");
    
        StartCoroutine(NetworkManager.Instance.GetUserGameRecords(
            // 성공 콜백
            (gameRecords) => {
                Debug.Log($"게임 기록 로드 성공: {gameRecords.Count}개의 기록");
            
                // 각 게임 기록 상세 정보 로깅
                for (int i = 0; i < gameRecords.Count; i++)
                {
                    var record = gameRecords[i];
                    Debug.Log($"게임 #{i+1}: roomId={record.roomId}, " +
                              $"winner={record.winner}, " +
                              $"시작={record.createdAt}, " +
                              $"종료={record.finishedAt}");
                }
            
                // 이 시점에서는 셀을 생성하지 않음
                Debug.Log("셀 생성 단계는 건너뜀 (디버그 모드)");
            },
            // 실패 콜백
            () => {
                Debug.LogError("게임 기록 로드 실패");
            }
        ));
    }

    /// <summary>
    /// 서버에서 게임 기록 데이터 가져오기
    /// </summary>
    private void LoadGameRecords()
    {
        StartCoroutine(NetworkManager.Instance.GetUserGameRecords(
            // 성공 콜백
            (gameRecords) => {
                if (gameRecords == null || gameRecords.Count == 0)
                {
                    Debug.LogWarning("게임 기록이 없습니다.");
                    return;
                }

                gameRecordList = new List<GameSummary>(gameRecords);
                
                // 날짜 기준 내림차순 정렬 (최신 게임이 위에 오도록)
                gameRecordList.Sort((a, b) => 
                    DateTime.Parse(b.finishedAt).CompareTo(DateTime.Parse(a.finishedAt)));
                
                InitializeCells();
            },
            // 실패 콜백
            () => {
                Debug.LogError("게임 기록 로드 실패");
                HGameManager.Instance.OpenConfirmPanel("게임 기록을 불러오는데 실패했습니다.", null);
            }
        ));
    }

    /// <summary>
    /// 스크롤 뷰 및 초기 셀 생성
    /// </summary>
    private void InitializeCells()
    {
        // CellSize 설정, Scroll Rect의 Content 사이즈 조절
        cellSize = cellHeight + cellSpacing;
        scrollRect.content.sizeDelta = new Vector2(0, gameRecordList.Count * cellSize);

        // visibleCellList 리스트 초기화 
        visibleCellList = new LinkedList<GameRecordCell>();

        // Content의 anchoredPosition 설정
        scrollRect.content.anchoredPosition = Vector2.zero;

        // Content의 anchoredPosition을 기준으로 셀 생성
        var (start, end) = GetVisibleIndexRange();
        for (int i = start; i < end; i++)
        {
            var gameRecordCell = CreateCell(i);
            visibleCellList.AddLast(gameRecordCell);
        }
    }

    private (int startIndex, int endIndex) GetVisibleIndexRange()
    {
        float visibleHeight = scrollRectRectTransform.rect.height;

        // visibleCount 값 설정 : 화면에 보여질 셀 갯수 + 여유 셀 추가
        int visibleCount = Mathf.CeilToInt(visibleHeight / cellSize) + cellBuffer;

        // startIndex 값 설정 : 셀을 생성 시작 인덱스 
        int startIndex = Mathf.Max(0, Mathf.FloorToInt(scrollRect.content.anchoredPosition.y / cellSize) - 1);

        // endIndex 값 설정 : 최대로 만들어야하는 셀의 갯수
        int endIndex = Mathf.Min(gameRecordList.Count, startIndex + visibleCount);

        return (startIndex, endIndex);
    }

    private GameRecordCell CreateCell(int index)
    {
        // 오브젝트 풀링을 사용하는 경우 GameRecordCellObjectPool에서 가져오도록 수정
        //var cellObject = Instantiate(gameCellPrefab);
        var cellObject = ObjectPool.Instance.GetObject();
        var gameRecordCell = cellObject.GetComponent<GameRecordCell>();

        gameRecordCell.SetData(gameRecordList[index], index);
        gameRecordCell.OnClick = () => OnCellClicked(gameRecordList[index].roomId);

        gameRecordCell.transform.localPosition = new Vector3(0, -index * cellSize, 0);
        return gameRecordCell;
    }

    private void OnCellClicked(string roomId)
    {
        Debug.Log($"게임 기록 선택: {roomId}");
        OnGameRecordSelected?.Invoke(roomId);
    }

    private void OnValueChanged(Vector2 value)
    {
        if (prevScrollRectYValue < value.y) ScrollUp();
        else ScrollDown();
        prevScrollRectYValue = value.y;
    }

    private void ScrollUp()
    {
        // 위로 스크롤
        if (visibleCellList.Count == 0) return;
        
        var firstCell = visibleCellList.First.Value;
        var prevFirstCellIndex = firstCell.Index - 1;

        if (IsVisibleIndex(prevFirstCellIndex))
        {
            var newCell = CreateCell(prevFirstCellIndex);
            visibleCellList.AddFirst(newCell);
        }

        var lastCell = visibleCellList.Last.Value;
        if (!IsVisibleIndex(lastCell.Index))
        {
            // 오브젝트 풀링 사용 시 GameRecordCellObjectPool.Instance.ReturnObject(lastCell.gameObject);
            //Destroy(lastCell.gameObject);
            ObjectPool.Instance.ReturnObject(lastCell.gameObject);
            visibleCellList.RemoveLast();
        }
    }

    private void ScrollDown()
    {
        // 아래로 스크롤
        if (visibleCellList.Count == 0) return;
        
        var lastCell = visibleCellList.Last.Value;
        var nextLastCellIndex = lastCell.Index + 1;

        if (IsVisibleIndex(nextLastCellIndex))
        {
            var newCell = CreateCell(nextLastCellIndex);
            visibleCellList.AddLast(newCell);
        }

        var firstCell = visibleCellList.First.Value;
        if (!IsVisibleIndex(firstCell.Index))
        {
            // 오브젝트 풀링 사용 시 GameRecordCellObjectPool.Instance.ReturnObject(firstCell.gameObject);
            //Destroy(firstCell.gameObject);
            ObjectPool.Instance.ReturnObject(firstCell.gameObject);
            visibleCellList.RemoveFirst();
        }
    }

    private bool IsVisibleIndex(int index)
    {
        var (startIndex, endIndex) = GetVisibleIndexRange();
        return index >= startIndex && index < endIndex;
    }

    public void OcClickCloseButton()
    {
        Destroy(gameObject);
    }
}