using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class HYConstants
{
    public const int BoardSize = 15;
    public const float GridAreaRatio = 0.91f; // 전체 크기 대비 격자 영역의 비율, 632 / 692 * 100
    public enum GameType {SinglePlay, DualPlay, MultiPlay}

    public enum PlayerType { BlackPlayer, WhitePlayer }
    public enum MarkerType { None, Black , White }
    public enum GameResult{None, BlackWin, WhiteWin}
}
