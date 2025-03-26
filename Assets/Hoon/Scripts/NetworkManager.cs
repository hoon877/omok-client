using System;
using System.Collections;
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
                    HGameManager.Instance.OpenConfirmPanel("This user already exists.", () =>
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
                HGameManager.Instance.OpenConfirmPanel("Signup Success.", () =>
                {
                    success?.Invoke();
                });
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
                    HGameManager.Instance.OpenConfirmPanel("ID is invalid.", () =>
                    {
                        failure?.Invoke(0);
                    });
                }
                else if (result.result == 1)
                {
                    Debug.Log("실패");
                    // 패스워드가 유효하지 않음
                    HGameManager.Instance.OpenConfirmPanel("Password is invalid.", () =>
                    {
                        failure?.Invoke(1);
                    });
                }
                else if (result.result == 2)
                {
                    Debug.Log("성공");
                    // 성공
                    HGameManager.Instance.OpenConfirmPanel("Login Success.", () =>
                    {
                        success?.Invoke();
                    });
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
    
    public IEnumerator AddCoin(Coin coin, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(coin);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(HConstants.ServerURL + "/users/addcoin", UnityWebRequest.kHttpVerbPOST))
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
                HGameManager.Instance.OpenConfirmPanel("Get Coin.", () =>
                {
                    success?.Invoke();
                });
            }
        }
    }
}
