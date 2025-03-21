using UnityEngine;

public class RenjuRuleChecker : MonoBehaviour
{
    /// <summary>
    /// 주어진 보드에서 (row, col)에 player 돌을 두었을 때, Renju 금수(예: 장목, 쌍4, 쌍3)가 되는지 검사합니다.
    /// PlayerA와 PlayerB 모두에 대해 금수 판정을 수행합니다.
    /// </summary>
    public bool IsMoveForbidden(Constants.PlayerType[,] board, int row, int col, Constants.PlayerType player)
    {
        // 해당 자리가 비어있지 않으면(이미 돌이 놓여 있으면) 금수 판정을 하지 않습니다.
        if (board[row, col] != Constants.PlayerType.None)
            return false;

        // 임시 보드 복사 후 해당 위치에 돌을 둡니다.
        var boardCopy = (Constants.PlayerType[,])board.Clone();
        boardCopy[row, col] = player;

        // 장목(6개 이상 연속)이면 금수
        if (CheckOverline(boardCopy, row, col, player))
            return true;

        int openThreeCount = CountOpenThrees(boardCopy, row, col, player);
        int openFourCount = CountOpenFours(boardCopy, row, col, player);

        // 쌍4 또는 쌍3이면 금수 (여기서는 해당 패턴이 2개 이상이면 금수로 판정)
        if (openFourCount >= 2)
            return true;
        if (openThreeCount >= 2)
            return true;

        return false;
    }

    private bool CheckOverline(Constants.PlayerType[,] board, int row, int col, Constants.PlayerType player)
    {
        // 네 방향: 가로, 세로, 대각(우하), 역대각(우상)
        int[][] directions = new int[][] {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };

        foreach (var dir in directions)
        {
            int count = 1 + CountInDirection(board, row, col, dir[0], dir[1], player)
                          + CountInDirection(board, row, col, -dir[0], -dir[1], player);
            // 6개 이상이면 장목(금수)
            if (count > 5)
                return true;
        }
        return false;
    }

    private int CountInDirection(Constants.PlayerType[,] board, int row, int col, int dr, int dc, Constants.PlayerType player)
    {
        int count = 0;
        int r = row + dr;
        int c = col + dc;
        while (r >= 0 && r < board.GetLength(0) &&
               c >= 0 && c < board.GetLength(1) &&
               board[r, c] == player)
        {
            count++;
            r += dr;
            c += dc;
        }
        return count;
    }

    private int CountOpenThrees(Constants.PlayerType[,] board, int row, int col, Constants.PlayerType player)
    {
        int count = 0;
        // 예시로 "01110" 패턴(양쪽이 빈자리인 3목)을 open three로 간주합니다.
        string[] patterns = { "01110" };
        count += CountPatternsInAllDirections(board, row, col, player, patterns);
        return count;
    }

    private int CountOpenFours(Constants.PlayerType[,] board, int row, int col, Constants.PlayerType player)
    {
        int count = 0;
        // 예시로 "011110" 패턴(양쪽이 빈자리인 4목)을 open four로 간주합니다.
        string[] patterns = { "011110" };
        count += CountPatternsInAllDirections(board, row, col, player, patterns);
        return count;
    }

    private int CountPatternsInAllDirections(Constants.PlayerType[,] board, int row, int col, Constants.PlayerType player, string[] patterns)
    {
        int totalCount = 0;
        int[][] directions = new int[][] {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };

        foreach (var dir in directions)
        {
            // 해당 방향의 선을 문자열로 변환합니다.
            string line = GetLineString(board, row, col, dir[0], dir[1]);
            foreach (var pattern in patterns)
            {
                totalCount += CountPatternInLine(line, pattern);
            }
        }
        return totalCount;
    }

    private string GetLineString(Constants.PlayerType[,] board, int row, int col, int dr, int dc)
    {
        // 해당 방향의 전체 선을 문자열로 반환합니다.
        // 각 칸은 '0' (빈자리), '1' (PlayerA), '2' (PlayerB)로 표현합니다.
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        // 선의 시작점(가능한 가장 앞쪽) 찾기
        int startRow = row, startCol = col;
        while (startRow - dr >= 0 && startRow - dr < rows &&
               startCol - dc >= 0 && startCol - dc < cols)
        {
            startRow -= dr;
            startCol -= dc;
        }

        string line = "";
        int r = startRow;
        int c = startCol;
        while (r >= 0 && r < rows && c >= 0 && c < cols)
        {
            int value = (int)board[r, c]; // enum의 기본값 (None=0, PlayerA=1, PlayerB=2)
            line += value.ToString();
            r += dr;
            c += dc;
        }
        return line;
    }

    private int CountPatternInLine(string line, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = line.IndexOf(pattern, index)) != -1)
        {
            count++;
            index++; // 중첩된 패턴도 허용
        }
        return count;
    }
}

