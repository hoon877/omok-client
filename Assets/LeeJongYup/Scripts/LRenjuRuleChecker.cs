using UnityEngine;

public class LRenjuRuleChecker : MonoBehaviour
{
    // WRF 규칙에 따른 열린 3 (open three) 패턴: 양쪽에 여유가 있는 3목
    private static readonly string[] OpenThreePatterns = new string[]
    {
        "001110", // 양쪽에 한 칸씩 여유가 있는 3목
        "011100",
        "010110",
        "011010"
    };

    // WRF 규칙에 따른 열린 4 (open four) 패턴: 양쪽에 여유가 있는 4목
    private static readonly string[] OpenFourPatterns = new string[]
    {
        "0011110", // 양쪽에 여유가 있는 4목
        "0111100",
        "1011110",
        "0111101"
    };

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 주어진 보드에서 (row, col)에 돌을 두었을 때, WRF 규칙에 따른 Renju 금수(장목, 쌍삼, 쌍사, 사삼)를 검사합니다.
    /// WRF 규칙에서는 흑(보통 PlayerA)만 금수 판정을 받으므로, 백(PlayerB)은 항상 정상 수로 처리합니다.
    /// </summary>
    public bool IsMoveForbidden(LConstants.PlayerType[,] board, int row, int col, LConstants.PlayerType player)
    {
        // WRF 규칙에 따라 백은 금수 판정 대상이 아님.
        if (player != LConstants.PlayerType.PlayerA)
            return false;

        // 이미 돌이 놓여 있으면 판정하지 않음.
        if (board[row, col] != LConstants.PlayerType.None)
            return false;

        // 임시 보드 복사 후 해당 위치에 돌을 둡니다.
        var boardCopy = (LConstants.PlayerType[,])board.Clone();
        boardCopy[row, col] = player;

        // 장목 검사: 6개 이상의 연속 돌이면 금수.
        if (CheckOverline(boardCopy, row, col, player))
            return true;

        // 방향별로 열린 3과 열린 4가 검출되는 횟수를 구합니다.
        int openThreeDirectionCount = CountDistinctPatternDirections(boardCopy, row, col, player, OpenThreePatterns);
        int openFourDirectionCount = CountDistinctPatternDirections(boardCopy, row, col, player, OpenFourPatterns);

        // 쌍사: 서로 다른 두 방향에서 열린 4가 있으면 금수.
        if (openFourDirectionCount >= 2)
            return true;
        // 쌍삼: 서로 다른 두 방향에서 열린 3이 있으면 금수.
        if (openThreeDirectionCount >= 2)
            return true;
        // 사삼: 한 방향에서 열린 3, 다른 방향에서 열린 4가 동시에 형성되면 금수.
        if (openThreeDirectionCount >= 1 && openFourDirectionCount >= 1)
            return true;

        return false;
    }

    /// <summary>
    /// 특정 방향에 대해 연속된 돌의 개수를 셉니다.
    /// </summary>
    private int CountInDirection(LConstants.PlayerType[,] board, int row, int col, int dr, int dc, LConstants.PlayerType player)
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

    /// <summary>
    /// 장목(Overline) 판정: 네 방향 모두에서 해당 돌을 기준으로 양쪽을 포함해 6개 이상의 돌이 연속되면 금수.
    /// </summary>
    private bool CheckOverline(LConstants.PlayerType[,] board, int row, int col, LConstants.PlayerType player)
    {
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
            if (count > 5)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 네 방향 각각에 대해, 지정된 패턴이 발견되는지 여부를 판단하여, 
    /// 발견된 방향의 개수를 반환합니다.
    /// (한 방향에서 여러 번 발견되어도 1회로 계산)
    /// </summary>
    private int CountDistinctPatternDirections(LConstants.PlayerType[,] board, int row, int col, LConstants.PlayerType player, string[] patterns)
    {
        int count = 0;
        int[][] directions = new int[][] {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };

        foreach (var dir in directions)
        {
            string line = GetLineString(board, row, col, dir[0], dir[1]);
            foreach (var pattern in patterns)
            {
                if (line.IndexOf(pattern) != -1)
                {
                    count++;
                    break; // 한 방향에서 패턴이 발견되면 1회로 계산
                }
            }
        }
        return count;
    }

    /// <summary>
    /// 특정 방향의 선을 문자열로 변환합니다.
    /// 각 칸은 '0'(빈자리), '1'(PlayerA, 흑), '2'(PlayerB, 백)로 표현됩니다.
    /// </summary>
    private string GetLineString(LConstants.PlayerType[,] board, int row, int col, int dr, int dc)
    {
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
            int value = (int)board[r, c]; // None=0, PlayerA(흑)=1, PlayerB(백)=2
            line += value.ToString();
            r += dr;
            c += dc;
        }
        return line;
    }
}
