using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const string ServerURL = "http://localhost:3000";
    public const string GameServerURL = "ws://localhost:3000";
    public enum MultiplayManagerState
    {
        CreateRoom, 
        JoinRoom,
        StartGame,
        ExitRoom,
        EndGame
    }
    
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
