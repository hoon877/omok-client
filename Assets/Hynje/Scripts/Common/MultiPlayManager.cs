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

// 게임 요약 정보 클래스
[Serializable]
public class GameSummary
{
    [JsonProperty("roomId")]
    public string roomId;
    
    [JsonProperty("winner")]
    public string winner;
    
    [JsonProperty("myPlayerType")]
    public string myPlayerType;
    
    [JsonProperty("createdAt")]
    public string createdAt;
    
    [JsonProperty("finishedAt")]
    public string finishedAt;
}

// 유저의 여러 게임 기록을 담는 클래스
[Serializable]
public class UserGameRecordsData
{
    [JsonProperty("games")]
    public List<GameSummary> games;
}

public class MultiPlayManager : IDisposable
{
    private SocketIOUnity _socket;
    private HYConstants.PlayerType _myPlayerType;
    private event Action<HConstants.MultiplayManagerState, string> _onMultiplayStateChanged;
    public Action<MoveData> OnOpponentMove;
    public Action<string> OnGameEnded; // 정상적인 게임 종료 (승자 정보 전달)
    public Action<string> OnPlayerDisconnected; // 상대방이 비정상 종료했을 때 (승자 정보 전달)

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
    {
        Debug.Log("상대방이 게임에서 나갔습니다.");
        // 상대방이 비정상적으로 나갔을 때만 자동 승리 처리
        var winner = _myPlayerType == HYConstants.PlayerType.BlackPlayer ? "BlackWin" : "WhiteWin";
        _onMultiplayStateChanged?.Invoke(HConstants.MultiplayManagerState.EndGame, null);
        
        // 상대방 강제 종료 이벤트 발생
        OnPlayerDisconnected?.Invoke(winner);
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

    // 게임 종료 알림
    public void SendGameOver(string roomId, string winner, string myPlayerType)
    {
        _socket.Emit("gameOver", new { roomId, winner, myPlayerType });
    }

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