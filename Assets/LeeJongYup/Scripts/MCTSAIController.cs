using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class MCTSAIController
{
    // MCTS 반복 횟수 (필요에 따라 조정)
    public static int Iterations = 6000;
    // 탐색-활용 균형을 위한 상수 (보통 1.41 정도)
    public static float ExplorationConstant = 1.11f;

    // 현재 보드 상태로부터 후보 수를 생성합니다.
    // 미니맥스와 동일하게, 보드가 비어 있으면 중앙 위치를 반환하고,
    // 이미 돌이 놓인 셀 주변의 빈 칸만 후보로 추가합니다.
    private static List<(int row, int col)> GenerateMoves(Constants.PlayerType[,] board)
    {
        List<(int row, int col)> moves = new List<(int row, int col)>();
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        bool boardEmpty = true;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] != Constants.PlayerType.None)
                {
                    boardEmpty = false;
                    break;
                }
            }
            if (!boardEmpty)
                break;
        }
        if (boardEmpty)
        {
            moves.Add((rows / 2, cols / 2));
            return moves;
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] == Constants.PlayerType.None)
                {
                    bool adjacent = false;
                    for (int di = -1; di <= 1 && !adjacent; di++)
                    {
                        for (int dj = -1; dj <= 1 && !adjacent; dj++)
                        {
                            int ni = i + di, nj = j + dj;
                            if (ni >= 0 && ni < rows && nj >= 0 && nj < cols && board[ni, nj] != Constants.PlayerType.None)
                            {
                                adjacent = true;
                            }
                        }
                    }
                    if (adjacent)
                        moves.Add((i, j));
                }
            }
        }
        return moves;
    }

    // 보드 상태의 깊은 복사
    private static Constants.PlayerType[,] CopyBoard(Constants.PlayerType[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        Constants.PlayerType[,] newBoard = new Constants.PlayerType[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newBoard[i, j] = board[i, j];
            }
        }
        return newBoard;
    }

    // 현재 보드 상태에서 몇 개의 돌이 놓였는지 세어, 현재 턴의 플레이어를 결정합니다.
    // 기본적으로 PlayerA가 선공이라고 가정합니다.
    private static Constants.PlayerType GetCurrentPlayer(Constants.PlayerType[,] board)
    {
        int count = 0;
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] != Constants.PlayerType.None)
                    count++;
            }
        }
        return (count % 2 == 0) ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    // MCTS 트리의 노드를 표현하는 내부 클래스
    private class MCTSNode
    {
        public Constants.PlayerType[,] board;
        public MCTSNode parent;
        public (int row, int col) move; // 이 노드에 도달하게 한 마지막 수 (루트는 (-1,-1))
        public List<MCTSNode> children = new List<MCTSNode>();
        public int wins;   // 시뮬레이션에서 승리한 횟수 (AI가 승리한 경우)
        public int visits; // 해당 노드가 선택된 횟수

        public MCTSNode(Constants.PlayerType[,] board, MCTSNode parent, (int row, int col) move)
        {
            this.board = board;
            this.parent = parent;
            this.move = move;
            wins = 0;
            visits = 0;
        }

        // 자식 노드로 확장 가능한지 판단 (가능한 모든 후보 수와 비교)
        public bool IsFullyExpanded()
        {
            var moves = GenerateMoves(board);
            return children.Count >= moves.Count;
        }

        // 현재 보드가 종료 상태인지 판단
        public bool IsTerminal()
        {
            return IsTerminalState(board);
        }

        // UCT 값을 계산합니다.
        public double UCTValue(float explorationConstant)
        {
            if (visits == 0) return double.MaxValue;
            double winRate = (double)wins / visits;
            double exploration = explorationConstant * Math.Sqrt(Math.Log(parent.visits) / visits);
            return winRate + exploration;
        }
    }

    // 시뮬레이션 단계: 현재 보드 상태에서 무작위로 게임을 진행한 후 승리한 플레이어를 반환합니다.
    // AI는 PlayerB로 가정합니다.
    private static Constants.PlayerType DefaultPolicy(Constants.PlayerType[,] board)
    {
        Constants.PlayerType[,] simulationBoard = CopyBoard(board);
        Constants.PlayerType currentPlayer = GetCurrentPlayer(simulationBoard);
        while (!IsTerminalState(simulationBoard))
        {
            List<(int row, int col)> moves = GenerateMoves(simulationBoard);
            if (moves.Count == 0) break;
            var move = moves[UnityEngine.Random.Range(0, moves.Count)];
            simulationBoard[move.row, move.col] = currentPlayer;
            currentPlayer = (currentPlayer == Constants.PlayerType.PlayerA) ? Constants.PlayerType.PlayerB : Constants.PlayerType.PlayerA;
        }
        if (CheckGameWin(Constants.PlayerType.PlayerB, simulationBoard))
            return Constants.PlayerType.PlayerB;
        if (CheckGameWin(Constants.PlayerType.PlayerA, simulationBoard))
            return Constants.PlayerType.PlayerA;
        return Constants.PlayerType.None;
    }

    // 역전파 단계: 시뮬레이션 결과를 루트까지 전파합니다.
    // 여기서는 AI가 PlayerB라고 가정하여, 시뮬레이션 결과가 PlayerB일 경우 승점을 추가합니다.
    private static void Backup(MCTSNode node, Constants.PlayerType simulationResult)
    {
        while (node != null)
        {
            node.visits++;
            if (simulationResult == Constants.PlayerType.PlayerB)
                node.wins++;
            node = node.parent;
        }
    }

    // 선택 및 확장 단계: 자식 노드가 모두 확장되어 있다면 UCT 값이 가장 높은 자식을 선택합니다.
    // 그렇지 않으면 미확장 후보 중 하나를 확장하여 반환합니다.
    private static MCTSNode TreePolicy(MCTSNode node)
    {
        while (!node.IsTerminal())
        {
            var moves = GenerateMoves(node.board);
            if (node.children.Count < moves.Count)
                return Expand(node);
            else
                node = BestChild(node);
        }
        return node;
    }

    // 확장 단계: 아직 확장되지 않은 후보 수 중 무작위로 선택하여 새로운 자식 노드를 생성합니다.
    private static MCTSNode Expand(MCTSNode node)
    {
        List<(int row, int col)> moves = GenerateMoves(node.board);
        HashSet<(int row, int col)> triedMoves = new HashSet<(int row, int col)>();
        foreach (var child in node.children)
        {
            triedMoves.Add(child.move);
        }
        List<(int row, int col)> untriedMoves = moves.FindAll(m => !triedMoves.Contains(m));
        var selectedMove = untriedMoves[UnityEngine.Random.Range(0, untriedMoves.Count)];
        Constants.PlayerType[,] newBoard = CopyBoard(node.board);
        Constants.PlayerType currentPlayer = GetCurrentPlayer(newBoard);
        newBoard[selectedMove.row, selectedMove.col] = currentPlayer;
        MCTSNode childNode = new MCTSNode(newBoard, node, selectedMove);
        node.children.Add(childNode);
        return childNode;
    }

    // UCT 공식을 이용해 자식 노드 중 가장 유망한 노드를 선택합니다.
    private static MCTSNode BestChild(MCTSNode node)
    {
        MCTSNode bestChild = null;
        double bestUCT = double.MinValue;
        foreach (var child in node.children)
        {
            double uct = child.UCTValue(ExplorationConstant);
            if (uct > bestUCT)
            {
                bestUCT = uct;
                bestChild = child;
            }
        }
        return bestChild;
    }

    // 메인 MCTS 함수: 주어진 반복 횟수만큼 트리 탐색 후 최적의 수를 결정합니다.
    // AI는 PlayerB로 가정합니다.
    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board)
    {
        MCTSNode root = new MCTSNode(CopyBoard(board), null, (-1, -1));
        for (int i = 0; i < Iterations; i++)
        {
            MCTSNode node = TreePolicy(root);
            Constants.PlayerType result = DefaultPolicy(node.board);
            Backup(node, result);
        }
        MCTSNode bestChild = null;
        int bestVisits = -1;
        foreach (var child in root.children)
        {
            if (child.visits > bestVisits)
            {
                bestVisits = child.visits;
                bestChild = child;
            }
        }
        if (bestChild != null)
            return bestChild.move;
        return null;
    }

    // 보드가 종료 상태인지 확인합니다.
    // 종료 상태란 어느 한쪽의 승리 또는 보드가 모두 채워진 경우입니다.
    private static bool IsTerminalState(Constants.PlayerType[,] board)
    {
        return CheckGameWin(Constants.PlayerType.PlayerA, board) ||
               CheckGameWin(Constants.PlayerType.PlayerB, board) ||
               IsAllBlocksPlaced(board);
    }

    // 게임 승리 여부를 판단합니다.
    private static bool CheckGameWin(Constants.PlayerType playerType, Constants.PlayerType[,] board)
    {
        int numRows = board.GetLength(0);
        int numCols = board.GetLength(1);
        int winCount = 5;

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                if (board[row, col] != playerType)
                    continue;

                // 가로 체크
                if (col <= numCols - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }

                // 세로 체크
                if (row <= numRows - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }

                // 대각선 (우하향)
                if (row <= numRows - winCount && col <= numCols - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }

                // 대각선 (우상향)
                if (row <= numRows - winCount && col >= winCount - 1)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col - i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }
            }
        }
        return false;
    }

    // 보드의 모든 칸이 채워졌는지 확인합니다.
    public static bool IsAllBlocksPlaced(Constants.PlayerType[,] board)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == Constants.PlayerType.None)
                    return false;
            }
        }
        return true;
    }
}
