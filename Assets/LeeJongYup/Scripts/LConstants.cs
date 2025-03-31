using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LConstants
{
    
    public enum PlayerType { None, PlayerA, PlayerB }
    public enum GameType { SinglePlayer, DualPlayer, MultiPlayer }
    public enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }
}
