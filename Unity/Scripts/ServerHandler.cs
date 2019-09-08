using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[System.Serializable]
public class IntUnityEvent : UnityEvent<ServerData>
{
    public ServerData data;
}

public class ServerHandler : MonoBehaviour
{
    public event OnLeaderboardDataFromServer OnGetUserDataAttemptForRequestComplete;

    private readonly WaitForSeconds waitToRefreshServerCall = new WaitForSeconds(2.00f);

    [HideInInspector] public bool isShowLogin = false;
    [HideInInspector] public bool isPasscodeAuthorized = false;

    public bool IsEnabled = false;

    private void Start()
    {
        
    }

    private UnityWebRequest ConnectToServer(string requestUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(PersistentModel.Instance.ServerURL + requestUrl);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.method = "GET";
        request.certificateHandler = new SSLAuth();

        return request;
    }

    public IEnumerator GetUserDataAttemptForRequest()
    {
        if (!IsEnabled)
        {
            OnGetUserDataAttemptForRequestComplete?.Invoke(true, null);
        }
        else
        {
            DebugLog.Trace("Server.GetUserDataAttemptForRequest()");

            string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email) + "&f=" + WWW.EscapeURL(PersistentModel.Instance.Name);
            requestUrl += "&r=" + WWW.EscapeURL(PersistentModel.Instance.Region) + "&o=" + WWW.EscapeURL(PersistentModel.Instance.Org);

            UnityWebRequest request = ConnectToServer("GetUserDataByEmail.php?" + requestUrl);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                DebugLog.Trace("UnityWebRequest.Error: " + request.error);

                // wait a few seconds before retry
                yield return waitToRefreshServerCall;

                GetUserDataAttemptForRequest();
            }
            else
            {
                ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

                // DebugLog.Trace("UnityWebRequest.RequestCompleted.resultData:" + resultData);

                if (resultData[0].success)
                {
                    string debugInfo = "";
                    // DebugLog.Trace("UnityWebRequest.Success");

                    debugInfo = "gamemode: " + resultData[0].gamemode;
                    debugInfo += "   challengeIndex: " + resultData[0].challengeIndex;
                    debugInfo += "   isTracksCompleted: " + resultData[0].isTracksCompleted;
                    debugInfo += "   trackTimeTotal: " + resultData[0].trackTimeTotal;
                    debugInfo += "   track_data: " + resultData[0].track_data;
                    debugInfo += "   completion: " + resultData[0].completion;
                    // DebugLog.Trace(debugInfo);

                    TrackData[] trackData = resultData[0].track_data;

                    PersistentModel.Instance.GameTrackData = new List<string>();

                    if (trackData.Length > 0)
                    {
                        for (int i = 0; i < trackData.Length; i++)
                        {
                            // this is so we can check what tracks user has played
                            // we dont want them to do any previously played tracks
                            PersistentModel.Instance.GameTrackData.Add(trackData[i].track_uid);
                        }
                    }

                    // save user game settings
                    PersistentModel.Instance.SetGameMode(resultData[0].gamemode);
                    PersistentModel.Instance.ChallengeCounter = resultData[0].challengeIndex;
                    PersistentModel.Instance.ChallengeIndex = resultData[0].challengeIndex;
                    PersistentModel.Instance.TotalChallengeTime = resultData[0].trackTimeTotal;
                    PersistentModel.Instance.HasReadInstructions = (resultData[0].challengeIndex > 0);
                    PersistentModel.Instance.SetTrackCompletion(resultData[0].completion);
                    PersistentModel.Instance.CurrentCircuitTime = resultData[0].currentCircuitTime;

                    // HARD RESET for Apple User
                    // if (PersistentModel.Instance.Email == "tester@apple.com") PersistentModel.Instance.Reset();

                    OnGetUserDataAttemptForRequestComplete?.Invoke(true, resultData[0]);
                }
            }
        }
    }

    public event OnUpdateDataFromServer OnUpdateUserChallengeIndexAttemptForRequestComplete;
    public void UpdateUserChallengeIndex()
    {
        if (!IsEnabled)
        {
            OnUpdateUserChallengeIndexAttemptForRequestComplete?.Invoke(true);

            return;
        }

        StartCoroutine(UpdateChallengeIndexRequest());
    }

    IEnumerator UpdateChallengeIndexRequest()
    {
        // if (PersistentModel.Instance.RunLocation == PersistentModel.RUN_LOCATION.Cocoa) yield break;

        // reset challenge time
        PersistentModel.Instance.ChallengeTime = 0f;

        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);
        //requestUrl += "&index=" + PersistentModel.Instance.ChallengeIndex;
        requestUrl += "&index=" + PersistentModel.Instance.ChallengeCounter;

        UnityWebRequest request = ConnectToServer("UpdateChallengeIndex.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            UpdateChallengeIndexRequest();
        }
        else
        {
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("UpdateUserChallengeIndex().success");
            else DebugLog.Trace("UpdateUserChallengeIndex().error");

            OnUpdateUserChallengeIndexAttemptForRequestComplete?.Invoke(resultData[0].success);
        }
    }
    
    // called when a challenge is completed
    public void StartNewGameUpdate()
    {
        if (!IsEnabled)
        {
            return;
        }

        StartCoroutine(OnStartNewGameUpdateAttemptForRequest());
    }

    IEnumerator OnStartNewGameUpdateAttemptForRequest()
    {
        DebugLog.Trace("OnStartNewGameUpdateAttemptForRequest()");

        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("StartNewGame.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            OnStartNewGameUpdateAttemptForRequest();
        }
        else
        {
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("StartNewGameUpdate().success");
            else DebugLog.Trace("StartNewGameUpdate().error");
        }
    }

    // called when a challenge is completed
    public void ChallengeCompleteUpdate(TrackData currentTrackData, IntUnityEvent callback)
    {
        if (!IsEnabled)
        {
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(OnChallengeCompleteUpdateAttemptForRequest(currentTrackData, callback));
    }

    IEnumerator OnChallengeCompleteUpdateAttemptForRequest(TrackData currentTrackData, IntUnityEvent callback)
    {
        DebugLog.Trace("OnChallengeCompleteUpdateAttemptForRequest()");

        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);
        requestUrl += "&mode=" + PersistentModel.Instance.GameModeID;
        // requestUrl += "&index=" + PersistentModel.Instance.ChallengeIndex;
        requestUrl += "&index=" + PersistentModel.Instance.ChallengeCounter;
        requestUrl += "&uid=" + currentTrackData.track_uid;
        requestUrl += "&type=" + currentTrackData.track_type;
        requestUrl += "&time=" + currentTrackData.track_time;

        Debug.Log("ChallengeCompleteUpdate.php ?");
        Debug.Log("requestUrl: " + requestUrl);

        // reset challenge time
        PersistentModel.Instance.ChallengeTime = 0f;

        UnityWebRequest request = ConnectToServer("ChallengeCompleteUpdate.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            OnChallengeCompleteUpdateAttemptForRequest(currentTrackData, callback);
        }
        else
        {
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("ChallengeCompleteUpdate().success");
            else DebugLog.Trace("ChallengeCompleteUpdate().error");

            // update completion and record times
            PersistentModel.Instance.SetTrackCompletion(resultData[0].completion);
            PersistentModel.Instance.SetCircuitTimes(resultData[0].circuitRecordTimes);
            PersistentModel.Instance.CurrentCircuitTime = (resultData[0].currentCircuitTime);

            callback.data = resultData[0];
            callback?.Invoke(callback.data);
        }
    }


    // called when a challenge is completed
    public void CircuitCompleteUpdate(IntUnityEvent callback)
    {
        if (!IsEnabled) return;
        
        StartCoroutine(OnCircuitCompleteUpdateAttemptForRequest(callback));
    }

    IEnumerator OnCircuitCompleteUpdateAttemptForRequest(IntUnityEvent callback)
    {
        DebugLog.Trace("OnCircuitCompleteUpdateAttemptForRequest()");

        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);
        requestUrl += "&mode=" + PersistentModel.Instance.GameModeID;

        // Debug.Log("CircuitCompleteUpdate.php ?");
        // Debug.Log("requestUrl: " + requestUrl);

        // reset challenge time
        PersistentModel.Instance.ChallengeTime = 0f;

        UnityWebRequest request = ConnectToServer("CircuitCompleteUpdate.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            OnCircuitCompleteUpdateAttemptForRequest(callback);
        }
        else
        {
            Debug.Log("downloadHandler.text: " + request.downloadHandler.text);

            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("CircuitCompleteUpdate().success");
            else DebugLog.Trace("CircuitCompleteUpdate().error");

            callback.data = resultData[0];
            callback?.Invoke(callback.data);
        }
    }

    // Get Record Time by Circuit
    public void GetRecordTimeByGameMode(string gameMode, IntUnityEvent callback)
    {
        if (!IsEnabled)
        {
            callback?.Invoke(null);

            return;
        }

        StartCoroutine(OnGetRecordTimeByGameModeAttemptForRequest(gameMode, callback));
    }

    IEnumerator OnGetRecordTimeByGameModeAttemptForRequest(string gameMode, IntUnityEvent callback)
    {
        DebugLog.Trace("OnGetRecordTimeByGameModeAttemptForRequest()");

        string requestUrl = "g=" + gameMode;

        UnityWebRequest request = ConnectToServer("GetRecordTimeByGameMode.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            OnGetRecordTimeByGameModeAttemptForRequest(gameMode, callback);
        }
        else
        {
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("GetRecordTimeByGameMode().success");
            else DebugLog.Trace("GetRecordTimeByGameMode().error");

            DebugLog.Trace("GetRecordTimeByGameMode.resultData[0].currentTrackRecordTime:" + resultData[0].currentTrackRecordTime);
            PersistentModel.Instance.CurrentTrackRecordTime = resultData[0].currentTrackRecordTime;

            callback.data = resultData[0];
            callback?.Invoke(callback.data);
        }
    }

    // Get Record Time by Circuit
    public void GetRecordTimeByTrackID(string trackID, IntUnityEvent callback)
    {
        if (!IsEnabled)
        {
            callback?.Invoke(null);

            return;
        }

        StartCoroutine(OnGetRecordTimeByTrackIDAttemptForRequest(trackID, callback));
    }

    IEnumerator OnGetRecordTimeByTrackIDAttemptForRequest(string trackID, IntUnityEvent callback)
    {
        DebugLog.Trace("OnGetRecordTimeByTrackIDAttemptForRequest()");

        string requestUrl = "trackid=" + trackID;

        UnityWebRequest request = ConnectToServer("GetRecordTimeByTrackID.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            OnGetRecordTimeByTrackIDAttemptForRequest(trackID, callback);
        }
        else
        {
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success) DebugLog.Trace("GetRecordTimeByTrackID().success");
            else DebugLog.Trace("GetRecordTimeByTrackID().error");

            callback.data = resultData[0];
            callback?.Invoke(callback.data);
        }
    }


    #region LEADERBOARDS
    // Leaderboard Server Events
    public event OnLeaderboardDataFromServer OnGetTop10FromServerComplete;
    public event OnLeaderboardDataFromServer OnGetTop10ByGameModeFromServerComplete;
    public event OnLeaderboardDataFromServer OnGetRangeFromServerComplete;
    public event OnLeaderboardDataFromServer OnGetRegionRangeFromServerComplete;
    public event OnLeaderboardDataFromServer OnGetOrgRangeFromServerComplete;

    public void RefreshLeaderboardData()
    {
        // if (PersistentModel.Instance.RunLocation == PersistentModel.RUN_LOCATION.Cocoa) return;

        // all we need at start are the leaderboard rank values
        GrabRanksFromServer();
    }

    public void GrabRanksFromServer()
    {
        StartCoroutine(GrabRankFromServerWaitForRequest());
    }

    IEnumerator GrabRankFromServerWaitForRequest()
    {
        if (PersistentModel.Instance.Email.Length == 0)
        {
            PersistentModel.Instance.Email = "john@sweetrush.com";
        }

        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("GetRank.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabRankFromServerWaitForRequest();
        }
        else
        {
            // Show results as text
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData[0].success)
            {
                PersistentModel.Instance.UserRank = resultData[0].rank;
                PersistentModel.Instance.UserRegionRank = resultData[0].regionRank;
                PersistentModel.Instance.UserOrgRank = resultData[0].orgRank;
            }
            else
            {
                // Debug.Log("No User Rank");
            }
        }
    }

    public void GrabTop10FromServer()
    {
        DebugLog.Trace("GrabTop10FromServer()");
        StartCoroutine(GrabTop10FromServerWaitForRequest());
    }

    IEnumerator GrabTop10FromServerWaitForRequest()
    {
        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("GetOverallTimes.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("GrabTop10FromServer.UnityWebRequest.Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabTop10FromServerWaitForRequest();
        }
        else
        {
            // Show results as text
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            OnGetTop10FromServerComplete?.Invoke(true, resultData[0]);
        }
    }


    public void GrabTop10ByGameModeFromServer(string gamemode)
    {
        DebugLog.Trace("GrabTop10ByGameModeFromServer()");

        StartCoroutine(GrabTop10ByGameModeFromServerWaitForRequest(gamemode));
    }

    IEnumerator GrabTop10ByGameModeFromServerWaitForRequest(string gamemode)
    {
        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);
        requestUrl += "&g=" + WWW.EscapeURL(gamemode);

        UnityWebRequest request = ConnectToServer("GetTimesByGameMode.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("GrabTop10ByGameModeFromServer.UnityWebRequest.Error: " + request.error);

            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabTop10ByGameModeFromServerWaitForRequest(gamemode);
        }
        else
        {
            // Show results as text
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            OnGetTop10ByGameModeFromServerComplete?.Invoke(true, resultData[0]);
        }
    }

    public void GrabRangeFromServer()
    {
        StartCoroutine(GrabRangeFromServerWaitForRequest());
    }

    IEnumerator GrabRangeFromServerWaitForRequest()
    {
        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("GetRange.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);
      
            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabRangeFromServerWaitForRequest();
        }
        else
        {
            // Show results as text
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData.Length > 0)
            {
                PersistentModel.Instance.PlayersRangeList = new List<string>();
                PersistentModel.Instance.ScoresRangeList = new List<string>();

                for (int i = 0; i < resultData.Length; i++)
                {
                    PersistentModel.Instance.PlayersRangeList.Add(resultData[i].fullname);
                    PersistentModel.Instance.ScoresRangeList.Add(resultData[i].total_time.ToString());
                }
            }

            // dispatch event
            OnGetRangeFromServerComplete?.Invoke(true, null);
        }
    }

    public void GrabRegionRangeFromServer()
    {
        StartCoroutine(GrabRegionRangeFromServerWaitForRequest());
    }

    IEnumerator GrabRegionRangeFromServerWaitForRequest()
    {
        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("GetRegionRange.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);
     
            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabRegionRangeFromServerWaitForRequest();
        }
        else
        {
            // Show results as text
            Debug.Log(request.downloadHandler.text);
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData.Length > 0)
            {
                PersistentModel.Instance.PlayersRegionRangeList = new List<string>();
                PersistentModel.Instance.ScoresRegionRangeList = new List<string>();

                for (int i = 0; i < resultData.Length; i++)
                {
                    //Debug.Log("resultData[i].fullname:" + resultData[i].fullname);
                    PersistentModel.Instance.PlayersRegionRangeList.Add(resultData[i].fullname);
                    PersistentModel.Instance.ScoresRegionRangeList.Add(resultData[i].total_time.ToString());
                }
            }

            OnGetRegionRangeFromServerComplete?.Invoke(true, null);
        }
    }

    public void GrabOrgRangeFromServer()
    {
        StartCoroutine(GrabOrgRangeFromServerWaitForRequest());
    }

    IEnumerator GrabOrgRangeFromServerWaitForRequest()
    {
        string requestUrl = "e=" + WWW.EscapeURL(PersistentModel.Instance.Email);

        UnityWebRequest request = ConnectToServer("GetOrgRange.php?" + requestUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            DebugLog.Trace("UnityWebRequest Error: " + request.error);
       
            // wait a few seconds before retry
            yield return waitToRefreshServerCall;

            GrabOrgRangeFromServerWaitForRequest();
        }
        else
        {
            // Show results as text
            ServerData[] resultData = JsonHelper.FromJsonWrapped<ServerData>(request.downloadHandler.text);

            if (resultData.Length > 0)
            {
                PersistentModel.Instance.PlayersOrgRangeList = new List<string>();
                PersistentModel.Instance.ScoresOrgRangeList = new List<string>();

                for (int i = 0; i < resultData.Length; i++)
                {
                    PersistentModel.Instance.PlayersOrgRangeList.Add(resultData[i].fullname);
                    PersistentModel.Instance.ScoresOrgRangeList.Add(resultData[i].total_time.ToString());
                }
            }

            OnGetOrgRangeFromServerComplete?.Invoke(true, null);
        }
    }
    #endregion // LEADERBOARDS

}
