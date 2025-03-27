using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LeeGyuDae
{
    public static class RankingBoardNetworkManager
    {
        public static IEnumerator GetRecords(Action<List<UserRecordData>> successCallback)
        {
            using (UnityWebRequest www = new UnityWebRequest("http://localhost:3000/records/getallrecords", UnityWebRequest.kHttpVerbGET))
            {
                www.downloadHandler = new DownloadHandlerBuffer();

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("유저 기록 데이터 요청 실패: " + www.error);
                }
                else
                {
                    string result = www.downloadHandler.text;
                    Debug.Log("유저 기록 데이터 수신: " + result);

                    UserRecordDataWrapper dataWrapper = JsonUtility.FromJson<UserRecordDataWrapper>(result);
                    successCallback?.Invoke(dataWrapper.recordsData);
                }
            }
        }
    }

    [Serializable]
    public class UserRecordDataWrapper
    {
        public List<UserRecordData> recordsData;
    }
}


