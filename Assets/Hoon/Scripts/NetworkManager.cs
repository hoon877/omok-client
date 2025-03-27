using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;


public class NetworkManager : HSingleton<NetworkManager>
{
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    public IEnumerator Signup(SignupData signupData, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(signupData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/signup", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);

                if (www.responseCode == 409)
                {
                    // TODO: 중복 사용자 생성 팝업 표시
                    Debug.Log("중복사용자");
                    HGameManager.Instance.OpenConfirmPanel("This user already exists.", () => { failure?.Invoke(); });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                Debug.Log("Result: " + result);

                // 회원가입 성공 팝업 표시
                HGameManager.Instance.OpenConfirmPanel("Signup Success.", () => { success?.Invoke(); });
            }
        }
    }

    public IEnumerator Signin(LoginData loginData, Action success, Action<int> failure)
    {
        string jsonString = JsonUtility.ToJson(loginData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/signin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {

            }
            else
            {
                var cookie = www.GetResponseHeader("set-cookie");
                if (!string.IsNullOrEmpty(cookie))
                {
                    int lastIndex = cookie.LastIndexOf(";");
                    string sid = cookie.Substring(0, lastIndex);
                    PlayerPrefs.SetString("sid", sid);
                }

                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<LoginResult>(resultString);

                if (result.result == 0)
                {
                    Debug.Log("실패");
                    // 유저네임 유효하지 않음
                    HGameManager.Instance.OpenConfirmPanel("ID is invalid.", () => { failure?.Invoke(0); });
                }
                else if (result.result == 1)
                {
                    Debug.Log("실패");
                    // 패스워드가 유효하지 않음
                    HGameManager.Instance.OpenConfirmPanel("Password is invalid.", () => { failure?.Invoke(1); });
                }
                else if (result.result == 2)
                {
                    Debug.Log("성공");
                    // 성공
                    HGameManager.Instance.OpenConfirmPanel("Login Success.", () => { success?.Invoke(); });
                }
            }
        }
    }

    public IEnumerator GetScore(Action<ProfileResult> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/getprofile", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();

            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                }

                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var userProfileResult = JsonUtility.FromJson<ProfileResult>(result);

                Debug.Log(userProfileResult.profileIndex);

                success?.Invoke(userProfileResult);
            }
        }
    }

    public IEnumerator GetCoin(Action<Coin> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/coin", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();

            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                }

                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var userCoin = JsonUtility.FromJson<Coin>(result);

                Debug.Log(userCoin.coin);
                Debug.Log(userCoin.nickname);
                success?.Invoke(userCoin);
            }
        }
    }
    
    public IEnumerator GetGameRecord(string roomId, Action<List<GameMove>> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/records/game/" + roomId, UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            // 인증 쿠키 설정
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"게임 기록 조회 실패: {www.error}");
                failure?.Invoke();
            }
            else
            {
                try
                {
                    string jsonResult = www.downloadHandler.text;
                    GameRecordData recordData = JsonUtility.FromJson<GameRecordData>(jsonResult);
                    success?.Invoke(recordData.gameRecord);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"게임 기록 파싱 오류: {ex.Message}");
                    failure?.Invoke();
                }
            }
        }
    }

    // 여러 게임 기록 조회 (옵션)
    public IEnumerator GetUserGameRecords(Action<List<GameSummary>> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/records/user", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            // 인증 쿠키 설정
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"유저 게임 기록 조회 실패: {www.error}");
                failure?.Invoke();
            }
            else
            {
                try
                {
                    string jsonResult = www.downloadHandler.text;
                    UserGameRecordsData records = JsonUtility.FromJson<UserGameRecordsData>(jsonResult);
                    // 비어있는 값 처리
                    if (records.games != null)
                    {
                        foreach (var game in records.games)
                        {
                            // 빈 값을 "test"로 채우기
                            game.roomId = string.IsNullOrEmpty(game.roomId) ? "empty" : game.roomId;
                            game.winner = string.IsNullOrEmpty(game.winner) ? "BlackWin" : game.winner;
                            game.myPlayerType = string.IsNullOrEmpty(game.myPlayerType) ? "BlackPlayer" : game.myPlayerType;
                            game.createdAt = string.IsNullOrEmpty(game.createdAt) ? DateTime.Now.ToString() : game.createdAt;
                            game.finishedAt = string.IsNullOrEmpty(game.finishedAt) ? DateTime.Now.ToString() : game.finishedAt;
                        }
                    }
                    else
                    {
                        // games 배열이 null인 경우 빈 목록 생성
                        records.games = new List<GameSummary>();
                    }
                    success?.Invoke(records.games);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"유저 게임 기록 파싱 오류: {ex.Message}");
                    failure?.Invoke();
                }
            }
        }
    }
}
