using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private LConstants.PlayerType[,] _board;
    public BlockController _blockController;

    public void SetBoard(LConstants.PlayerType[,] board)
    {
        _board = board;
    }
    
    
    
    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    public LConstants.GameResult CheckGameResult()
    {
        if (CheckGameWin(LConstants.PlayerType.PlayerA)) { return LConstants.GameResult.Win; }
        if (CheckGameWin(LConstants.PlayerType.PlayerB)) { return LConstants.GameResult.Lose; }
        if (MinimaxAIController.IsAllBlocksPlaced(_board)) { return LConstants.GameResult.Draw; }
        
        return LConstants.GameResult.None;
    }
    // 게임의 승패를 판단하는 함수
    private bool CheckGameWin(LConstants.PlayerType playerType)
    {
        int winCount = 5;
        int numRows = _board.GetLength(0);
        int numCols = _board.GetLength(1);

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                if (_board[row, col] != playerType)
                    continue;

                // 가로 방향 (오른쪽)
                if (col <= numCols - winCount)
                {
                    bool win = true;
                    (int, int)[] blocks = new (int, int)[winCount];
                    for (int i = 0; i < winCount; i++)
                    {
                        blocks[i] = (row, col + i);
                        if (_board[row, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        _blockController.SetBlockColor(playerType, blocks);
                        return true;
                    }
                }

                // 세로 방향 (아래쪽)
                if (row <= numRows - winCount)
                {
                    bool win = true;
                    (int, int)[] blocks = new (int, int)[winCount];
                    for (int i = 0; i < winCount; i++)
                    {
                        blocks[i] = (row + i, col);
                        if (_board[row + i, col] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        _blockController.SetBlockColor(playerType, blocks);
                        return true;
                    }
                }

                // 대각선 방향 (오른쪽 아래)
                if (row <= numRows - winCount && col <= numCols - winCount)
                {
                    bool win = true;
                    (int, int)[] blocks = new (int, int)[winCount];
                    for (int i = 0; i < winCount; i++)
                    {
                        blocks[i] = (row + i, col + i);
                        if (_board[row + i, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        _blockController.SetBlockColor(playerType, blocks);
                        return true;
                    }
                }

                // 역대각선 방향 (왼쪽 아래)
                if (row <= numRows - winCount && col >= winCount - 1)
                {
                    bool win = true;
                    (int, int)[] blocks = new (int, int)[winCount];
                    for (int i = 0; i < winCount; i++)
                    {
                        blocks[i] = (row + i, col - i);
                        if (_board[row + i, col - i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        _blockController.SetBlockColor(playerType, blocks);
                        return true;
                    }
                }
            }
        }
        return false;
    }

}
