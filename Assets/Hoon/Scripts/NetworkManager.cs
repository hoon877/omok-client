using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
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
    
    public IEnumerator Signout(Action success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/signout", UnityWebRequest.kHttpVerbPOST))
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
                Debug.Log("로그아웃 실패: " + www.error);
                HGameManager.Instance.OpenConfirmPanel("Logout Failed.", () =>
                {
                    failure?.Invoke();
                });
            }
            else
            {
                Debug.Log("로그아웃 성공");
            
                // 세션 삭제 후 로컬 저장된 sid 삭제
                PlayerPrefs.DeleteKey("sid");

                HGameManager.Instance.OpenConfirmPanel("Logout Success.", () =>
                {
                    success?.Invoke();
                });
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
                    Debug.Log($"게임 기록 응답: {jsonResult}");
                
                    //GameRecordData recordData = JsonUtility.FromJson<GameRecordData>(jsonResult);
                    GameRecordData recordData = JsonConvert.DeserializeObject<GameRecordData>(jsonResult);
                
                    if (recordData != null && recordData.gameRecord != null)
                    {
                        success?.Invoke(recordData.gameRecord);
                    }
                    else
                    {
                        Debug.LogError("게임 기록 데이터가 null입니다");
                        failure?.Invoke();
                    }
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
    
    
    public IEnumerator SaveProfile(ProfileResult profileResult, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(profileResult);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/saveprofile", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);

                if (www.responseCode == 400)
                {
                    Debug.Log("유효한 값을 입력해주세요");
                    HGameManager.Instance.OpenConfirmPanel("not a valid value", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    HGameManager.Instance.OpenConfirmPanel("Login required", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 404)
                {
                    Debug.Log("사용자를 찾을 수 없습니다.");
                    HGameManager.Instance.OpenConfirmPanel("User not found", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 500)
                {
                    Debug.Log("서버 오류 발생");
                    HGameManager.Instance.OpenConfirmPanel("Server error", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else
                {
                    Debug.Log("알 수 없는 오류 발생: " + www.responseCode);
                    HGameManager.Instance.OpenConfirmPanel("Unknown error", () =>
                    {
                        failure?.Invoke();
                    });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                Debug.Log("Result: " + result);
                
                // 코인 변경 팝업 표시
                HGameManager.Instance.OpenConfirmPanel("Change Profile.", () =>
                {
                    success?.Invoke();
                });
            }
        }
    }
    
    public IEnumerator PasswordChange(changePassword passwordData, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(passwordData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/changepassword", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);

                if (www.responseCode == 400)
                {
                    Debug.Log("모든 필드 값을 입력해주세요");
                    HGameManager.Instance.OpenConfirmPanel("not a valid value", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    HGameManager.Instance.OpenConfirmPanel("Login required", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 404)
                {
                    Debug.Log("사용자를 찾을 수 없습니다.");
                    HGameManager.Instance.OpenConfirmPanel("User not found", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else if (www.responseCode == 500)
                {
                    Debug.Log("서버 오류 발생");
                    HGameManager.Instance.OpenConfirmPanel("Server error", () =>
                    {
                        failure?.Invoke();
                    });
                }
                else
                {
                    Debug.Log("알 수 없는 오류 발생: " + www.responseCode);
                    HGameManager.Instance.OpenConfirmPanel("Unknown error", () =>
                    {
                        failure?.Invoke();
                    });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                Debug.Log("Result: " + result);
                
                // 회원가입 성공 팝업 표시
                HGameManager.Instance.OpenConfirmPanel("Password Change Success.", () =>
                {
                    success?.Invoke();
                });
            }
        }
    }
}
