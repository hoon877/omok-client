using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingBoardPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject scrollRectGameObject;    
    
    [Header("Cell Settings")]
    [SerializeField] private float cellHeight  = 150f;
    [SerializeField] private float cellSpacing = 20f;
    [SerializeField] private int   cellBuffer;
    
    [Header("Profile Images")]
    [SerializeField] private Sprite[] profileImageSprites;
    
    // Scroll Rect
    private ScrollRect    scrollRect;
    private RectTransform scrollRectRectTransform;

    private float prevScrollRectYValue = 1f;
    
    // User Data
    public List<UserRecordData> recordDataList = new List<UserRecordData>();
    private LinkedList<RankingBoardCell> visibleCellList;
    private int currentUserRankOrder;

    // Cell
    private float cellSize;

    private void Awake()
    {
        scrollRect    = scrollRectGameObject.GetComponent<ScrollRect>();
        scrollRectRectTransform = scrollRectGameObject.GetComponent<RectTransform>();
        
        scrollRect.onValueChanged.AddListener(OnValueChanged);
    }

    private void Start()
    {
        LoadData();
    }

    /// <summary>
    /// DB에서 유저 데이터 받아오는 함수
    /// </summary>
    private void LoadData()
    {
        // TODO: DB 에서 데이터 가져오기
        StartCoroutine(RankingBoardNetworkManager.GetRecords(
            successCallback: (data) =>
            {
                if (data == null || data.Count == 0)
                {
                    Debug.LogWarning("데이터가 비어있습니다.");
                    return;
                }
                
                recordDataList = new List<UserRecordData>(data);
                InitializeCells();
            }
        ));
        
        // TODO: 현재 유저의 인덱스 값 받아오기
    }

    /// <summary>
    /// 스크롤 뷰 및 초기 셀 생성
    /// </summary>
    private void InitializeCells()
    {
        // CellSize 설정, Scroll Rect 의 Content 사이즈 조절
        cellSize = cellHeight + cellSpacing;
        scrollRect.content.sizeDelta = new Vector2(0, recordDataList.Count * cellSize);

        // visibleCellList 리스트 초기화 
        visibleCellList = new LinkedList<RankingBoardCell>();
        
        // Content 의 anchoredPosition 설정
        scrollRect.content.anchoredPosition = new Vector2(0, currentUserRankOrder * cellSize);
        
        // Content 의 anchoredPosition 을 기준으로 셀 생성
        var (start, end) = GetVisibleIndexRange();
        for (int i = start; i < end; i++)
        {
            var rankingBoardCell = CreateCell(i);
            visibleCellList.AddLast(rankingBoardCell);
        }
    }
    
    private (int startIndex, int endIndex) GetVisibleIndexRange()
    {
        float visibleHeight = scrollRectRectTransform.rect.height;
        
        // visibleCount 값 설정 : 화면에 보여질 셀 갯수 + 여유 셀 추가
        int visibleCount = Mathf.CeilToInt(visibleHeight / cellSize) + cellBuffer;
        
        // startIndex 값 설정 : 셀을 생성 시작 인덱스 
        int startIndex   = Mathf.Max(0, Mathf.FloorToInt(scrollRect.content.anchoredPosition.y / cellSize ) - 1);
        
        // endIndex 값 설정 : 최대로 만들어야하는 셀의 갯수
        int endIndex = Mathf.Min(recordDataList.Count, startIndex + visibleCount);
        
        return (startIndex, endIndex);
    }

    private RankingBoardCell CreateCell(int index)
    {
        var cellObject = ObjectPool.Instance.GetObject();
        var rankingBoardCell = cellObject.GetComponent<RankingBoardCell>();
        
        rankingBoardCell.SetData(
            recordDataList[index],
            profileImageSprites[recordDataList[index].profileIndex],
            index);
        
        rankingBoardCell.transform.localPosition = new Vector3(0, - index * cellSize, 0);
        return rankingBoardCell;
    }

    private void OnValueChanged(Vector2 value)
    {
        if (prevScrollRectYValue < value.y) ScrollUp();
        else                                ScrollDown();
        prevScrollRectYValue = value.y;
    }
    
    private void ScrollUp()
    {
        // 위로 스크롤
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
            ObjectPool.Instance.ReturnObject(lastCell.gameObject);
            visibleCellList.RemoveLast();
        }
    }
    
    private void ScrollDown()
    {
        // 아래로 스크롤
        var lastCell = visibleCellList.Last.Value;
        var nextLastCellIndex = lastCell.Index + 1;

        if (IsVisibleIndex(nextLastCellIndex))
        {
            var newCell = CreateCell(nextLastCellIndex);
            visibleCellList.AddLast(newCell);
        }
            
        var fistCell = visibleCellList.First.Value;
        if (!IsVisibleIndex(fistCell.Index))
        {
            ObjectPool.Instance.ReturnObject(fistCell.gameObject);
            visibleCellList.RemoveFirst();
        }
    }

    private bool IsVisibleIndex(int index)
    {
        var (startIndex, endIndex) = GetVisibleIndexRange();
        return index >= startIndex && index < endIndex;
    }
}