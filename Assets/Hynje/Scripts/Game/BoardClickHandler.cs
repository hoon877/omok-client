using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardClickHandler : MonoBehaviour
{
    [SerializeField] private GameObject positionSelector;
    
    private Camera _mainCamera;
    
    // 보드 이미지 정보
    private SpriteRenderer _boardSprite;
    private float _gridCellSize;
    private Vector2 _boardWorldSize;
    private Vector2 _boardOrigin; // 보드의 좌측 하단 좌표
    
    private void Awake()
    {
        // 카메라 할당
        _mainCamera = Camera.main;

        // 스프라이트 렌더러 가져오기
        _boardSprite = GetComponent<SpriteRenderer>();

        // 보드 정보 초기화
        CalculateBoardDimensions();
    }

    private void Start()
    {
        positionSelector.SetActive(false);
    }

    private void CalculateBoardDimensions()
    {
        // 스프라이트의 실제 월드 크기 계산
        Vector2 spriteSize = new Vector2(_boardSprite.bounds.size.x,  _boardSprite.bounds.size.y);
        
        // 현재 오브젝트의 스케일 적용
        _boardWorldSize = new Vector2(
            spriteSize.x * transform.lossyScale.x,
            spriteSize.y * transform.lossyScale.y
        );

        // 보드 테두리를 제외한 실제 격자 영역 크기 
        Vector2 gridArea = _boardWorldSize * Constants.GridAreaRatio;
        
        // 격자 셀 하나의 크기 계산
        _gridCellSize = gridArea.x / (Constants.BoardSize - 1);
        
        // 보드의 좌측 하단 좌표 계산 (실제 격자의 시작점)
        Vector2 boardCenter = (Vector2)transform.position;
        Vector2 halfGridArea = gridArea / 2f;
        _boardOrigin = boardCenter - halfGridArea;

        //Debug.Log($"Board World Size: {_boardWorldSize}, Grid Cell Size: {_gridCellSize}");
        //Debug.Log($"Board Origin (Bottom-Left): {_boardOrigin}");
    }

    private void OnMouseDrag()
    {
        Vector2Int gridPos = GetGridPositionFromClick();

        if (IsValidGridPosition(gridPos))
        {
            SetPositionSelector(gridPos);
        }
    }

    private void OnMouseUp()
    {
        Vector2Int gridPos = GetGridPositionFromClick();
        //Debug.Log($"Clicked on grid: {gridPos}");
    }

    public (Vector2Int, Vector3) GetSelectedPosition()
    {
        var selectedPos = positionSelector.transform.position;
        var gridPos = GetGridPositionFromWorldPoint(selectedPos);
        return (gridPos, selectedPos);
    }
    
    private void SetPositionSelector(Vector2Int gridPos)
    {
        positionSelector.SetActive(true);
        positionSelector.transform.position = GetWorldPositionFromGrid(gridPos);
    }

    private Vector2Int GetGridPositionFromClick()
    {
        // 마우스 클릭 위치를 월드 좌표로 변환
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -_mainCamera.transform.position.z;
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
        
        // 보드 위의 상대적 위치 계산
        Vector2 relativePos = (Vector2)worldPos - _boardOrigin;
        
        // 그리드 인덱스 계산
        int x = Mathf.RoundToInt(relativePos.x / _gridCellSize);
        int y = Mathf.RoundToInt(relativePos.y / _gridCellSize);
        
        // 범위 제한
        x = Mathf.Clamp(x, 0, Constants.BoardSize - 1);
        y = Mathf.Clamp(y, 0, Constants.BoardSize - 1);
        
        return new Vector2Int(x, y);
    }
    
    // 월드 위치에서 그리드 위치 계산
    public Vector2Int GetGridPositionFromWorldPoint(Vector3 worldPos)
    {
        Vector2 relativePos = (Vector2)worldPos - _boardOrigin;
        int x = Mathf.RoundToInt(relativePos.x / _gridCellSize);
        int y = Mathf.RoundToInt(relativePos.y / _gridCellSize);
        return new Vector2Int(x, y);
    }

    // 그리드 위치를 월드 좌표로 변환
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        float x = _boardOrigin.x + (gridPos.x * _gridCellSize);
        float y = _boardOrigin.y + (gridPos.y * _gridCellSize);
        return new Vector3(x, y, 0);
    }
    
    // 유효한 그리드 위치인지 확인
    private bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < Constants.BoardSize && gridPos.y >= 0 && gridPos.y < Constants.BoardSize;
    }
}
