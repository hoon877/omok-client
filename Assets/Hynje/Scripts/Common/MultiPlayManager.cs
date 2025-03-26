using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocketIOClient;
using UnityEngine;

public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

public class PlayerData
{
    [JsonProperty("player")]
    public string player { get; set; }
    [JsonProperty("position")]
    public int position { get; set; }
}

public class MoveData
{
    [JsonProperty("position")] 
    public int position { get; set; }
}

public class GameOverData
{
    [JsonProperty("winner")]
    public string winner { get; set; }
}

[Serializable]
public class GameMove
{
    [JsonProperty("player")]
    public string player { get; set; }
    [JsonProperty("position")]
    public int position { get; set; }
    [JsonProperty("timestamp")]
    public string timestamp { get; set; }
}

public class GameRecordData
{
    [JsonProperty("gameRecord")]
    public List<GameMove> gameRecord { get; set; }
}

public class MultiPlayManager : IDisposable
{
    private SocketIOUnity _socket;
    private HYConstants.PlayerType _myPlayerType;
    private event Action<HConstants.MultiplayManagerState, string> _onMultiplayStateChanged;
    public Action<MoveData> OnOpponentMove;
    public Action<string> OnGameEnded; // winner 정보 전달
    
    public MultiPlayManager(Action<HConstants.MultiplayManagerState, string> onMultiplayStateChanged)
    {
        _onMultiplayStateChanged = onMultiplayStateChanged;
        
        var uri = new Uri(HConstants.GameServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        
        // 기본 게임룸 이벤트
        _socket.On("createRoom", CreateRoom);
        _socket.On("joinRoom", JoinRoom);
        _socket.On("startGame", StartGame);
        _socket.On("exitRoom", ExitRoom);
        _socket.On("endGame", EndGame);
        
        // 오목 게임 관련 이벤트
        _socket.On("doOpponent", DoOpponent);
        _socket.On("gameEnded", GameEnded);
        //_socket.On("gameRecord", GameRecord);
        
        _socket.Connect();
    }
    
    public void SetMyPlayerType(HYConstants.PlayerType playerType)
    {
        _myPlayerType = playerType;
    }
    
    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.CreateRoom, data.roomId);
    }

    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.JoinRoom, data.roomId);
    }

    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.StartGame, data.roomId);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.ExitRoom, null);
    }

    private void EndGame(SocketIOResponse response)
    {Debug.Log("상대방이 게임에서 나갔습니다.");
        // 상대방이 나갔을 때 자동 승리 처리
        var winner = _myPlayerType == HYConstants.PlayerType.BlackPlayer ? "BlackWin" : "WhiteWin";
        OnGameEnded?.Invoke(winner);
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.EndGame, null);
    }

    // 상대방의 착수 정보 수신
    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<MoveData>();
        OnOpponentMove?.Invoke(data);
    }

    // 게임 종료 정보 수신
    private void GameEnded(SocketIOResponse response)
    {
        var data = response.GetValue<GameOverData>();
        OnGameEnded?.Invoke(data.winner);
    }

    // // 게임 기보 수신
    // private void GameRecord(SocketIOResponse response)
    // {
    //     var data = response.GetValue<GameRecordData>();
    //     OnGameRecordReceived?.Invoke(data.gameRecord);
    // }

    // 플레이어 착수 정보 전송
    public void SendPlayerMove(string roomId, int position, string player)
    {
        _socket.Emit("doPlayer", new { roomId, position, player });
    }

    // 게임룸 나가기
    public void LeaveRoom(string roomId)
    {
        _socket.Emit("leaveRoom", new { roomId });
    }

    // 게임 종료 알림 (승리 시)
    public void SendGameOver(string roomId, string winner)
    {
        _socket.Emit("gameOver", new { roomId, winner });
    }

    // // 게임 기보 요청
    // public void GetGameRecord(string roomId)
    // {
    //     _socket.Emit("getGameRecord", roomId);
    // }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }
    }
}