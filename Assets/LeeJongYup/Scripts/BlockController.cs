using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    
    public delegate void OnBlockClicked(int row, int col);
    public OnBlockClicked OnBlockClickedDelegate;
    
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void InitBlocks()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].InitMarker(i, blockIndex =>
            {
                var clickedRow = blockIndex / 15;
                var clickedCol = blockIndex % 15;
                
                OnBlockClickedDelegate?.Invoke(clickedRow, clickedCol);
            });
        }
    }
    
    /// <summary>
    /// 특정 Block에 마커 표시하는 함수
    /// </summary>
    /// <param name="markerType">마커 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    public void PlaceMarker(Block.MarkerType markerType, int row, int col)
    {
        // row, col을 index로 변환
        var markerIndex = row * 15 + col;
        
        // Block에게 마커 표시
        blocks[markerIndex].SetMarker(markerType);
    }
    
    public void SetBlockColor(LConstants.PlayerType playerType,
        (int row, int col)[] blockPositions)
    {
        if (playerType == LConstants.PlayerType.None) return;

        foreach (var blockPosition in blockPositions)
        {
            var blockIndex = blockPosition.row * 15 + blockPosition.col;
            Color32 markerColor;
            if (playerType == LConstants.PlayerType.PlayerA)
                markerColor = new Color32(0, 166, 255, 255);
            else if (playerType == LConstants.PlayerType.PlayerB)
                markerColor = new Color32(255, 0, 94, 255);
            else
                markerColor = Color.black;

            blocks[blockIndex].SetColor(markerColor);
        }
    }
    
}
