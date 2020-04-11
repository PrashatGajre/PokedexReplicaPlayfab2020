using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabManager : Singleton<PlayFabManager>
{
    PlayFabAuthenticationContext context;
    static string PlayFabID = "A3DE4";
    public enum LoginState
    {
        Startup,
        Instantiated,
        Success,
        Failed,
    }

    public LoginState state = LoginState.Startup;
    public string playerGUID = "";
    public bool createNewPlayer = false;

    public string UserEmail { get; set; }
    public string UserPassword { get; set; }
    public string UserName { get; set; }
    public GameObject createPlayerPanel;


    private void Start()
    {
        // A3DE4
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = PlayFabID;
        }

        playerGUID = PlayerPrefs.GetString("PlayFabPlayerId", "");
        if (string.IsNullOrEmpty(playerGUID))
        {
            createPlayerPanel.SetActive(true);
            createNewPlayer = true;
        }
        else
        {
            createNewPlayer = false;
            createPlayerPanel.SetActive(false);
            UserEmail = playerGUID;
            LoginUsingCustomID();
        }
    }


    #region RegisterUser

    //public void RegisterNewUser()
    //{
    //    var registerRequest = new RegisterPlayFabUserRequest { Email = UserEmail, Password = UserPassword, Username = UserName, DisplayName = UserName };
    //    PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterUserSuccess, OnRegisterUserFailure);
    //}

    //private void OnRegisterUserSuccess(RegisterPlayFabUserResult registerResult)
    //{
    //    PlayerPrefs.SetString("PlayFabPlayerId", UserEmail);
    //    Debug.Log("Player Registered.");
    //    //registerResult.
    //}

    //private void OnRegisterUserFailure(PlayFabError error)
    //{ 
    //    Debug.Log("Failed to register. Error : " + error.ErrorMessage);
    //}

    #endregion

    private void GetPlayerProfile()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            AuthenticationContext = context,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        result => { UserName = result.PlayerProfile.DisplayName; Debug.Log(UserName); OnPlayerLoggedIn?.Invoke();},
        error => Debug.LogError(error.GenerateErrorReport()));
        
    }

    #region LoginUser
    public void LoginUsingCustomID()
    {
        state = LoginState.Instantiated;

        LoginWithCustomIDRequest loginRequest = new LoginWithCustomIDRequest { CustomId = UserEmail, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(loginRequest, OnLoginSuccess, OnLoginFailure);
    }

    public delegate void OnLoggedIn();
    public OnLoggedIn OnPlayerLoggedIn;

    private void OnLoginSuccess(LoginResult result)
    {
        state = LoginState.Success;
        if (createNewPlayer)
        {
            PlayerPrefs.SetString("PlayFabPlayerId", UserEmail);
            PlayerPrefs.Save();
            UpdateUserDisplayName(UserName);
            createPlayerPanel.SetActive(false);
        }
        context = result.AuthenticationContext;
        GetPlayerProfile();
        Debug.Log("Congratulations, you have logged into PlayFab!!");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        state = LoginState.Failed;
        Debug.LogWarning("Something went wront logging into PlayFab :(");
        Debug.LogError(error.GenerateErrorReport());
    }
    #endregion

    #region UpdatePlayerDisplayName

    public void UpdateUserDisplayName(string name)
    {
        UpdateUserTitleDisplayNameRequest updateUserTitleRequest = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
        PlayFabClientAPI.UpdateUserTitleDisplayName(updateUserTitleRequest, OnDiplsayNameUpdateSuccess, OnDiplsayNameUpdateFailure);
    }

    public delegate void OnNameUpdate();
    public OnNameUpdate OnPlayerNameUpdated;

    private void OnDiplsayNameUpdateSuccess(UpdateUserTitleDisplayNameResult updateUserTitleResult)
    {
        UserName = updateUserTitleResult.DisplayName;
        OnPlayerNameUpdated?.Invoke();
        Debug.Log("Display Name Updated!");
    }
    private void OnDiplsayNameUpdateFailure(PlayFabError playFabError)
    {
        Debug.Log("Error updating Display Name : " + playFabError.ErrorMessage);
    }

    #endregion

    #region LogEvent

    public void LogPlayerEvent(string eventName, Dictionary<string, object> eventData)
    {
        WriteClientPlayerEventRequest playerEventRequest = new WriteClientPlayerEventRequest { AuthenticationContext = context, EventName = eventName, Body = eventData, Timestamp = System.DateTime.Now };
        PlayFabClientAPI.WritePlayerEvent(playerEventRequest, OnEventLogged, OnEventLogFailure);
    }

    private void OnEventLogged(WriteEventResponse writeEventResponse)
    {
        Debug.Log("Event " + writeEventResponse.EventId + " registered.");
        Debug.Log(writeEventResponse.ToString());
    }
    private void OnEventLogFailure(PlayFabError error)
    {
        Debug.Log("Failed to log Player Event : " + error.ErrorMessage);
    }
    #endregion

    #region Get/UpdatePlayerStats

    public void GetPlayerStats(List<string> statsList)
    {
        GetPlayerStatisticsRequest statisticsRequest = new GetPlayerStatisticsRequest { AuthenticationContext = context, StatisticNames = statsList };
        PlayFabClientAPI.GetPlayerStatistics(statisticsRequest, OnStatsFetchSuccess, OnStatsFetchFailure);
    }

    public delegate void OnStatsUpdate(List<StatisticValue> statistics);
    public OnStatsUpdate OnStatsUpdated;

    private void OnStatsFetchSuccess(GetPlayerStatisticsResult statisticsResult)
    {
        OnStatsUpdated?.Invoke(statisticsResult.Statistics);
        foreach (StatisticValue statisticValue in statisticsResult.Statistics)
        {
            Debug.Log(statisticValue.StatisticName + " : " + statisticValue.Value);
        }
    }

    private void OnStatsFetchFailure(PlayFabError playFabError)
    {
        Debug.Log("Error Fetching Stats : " + playFabError.ErrorMessage);
    }

    public void UpdatePlayerStatistics(List<StatisticUpdate> statsUpdate)
    {
        UpdatePlayerStatisticsRequest statUpdateRequest = new UpdatePlayerStatisticsRequest { Statistics = statsUpdate };
        PlayFabClientAPI.UpdatePlayerStatistics(statUpdateRequest, OnStatsUpdateSuccess, OnStatsUpdateFailure);
    }

    private void OnStatsUpdateSuccess(UpdatePlayerStatisticsResult statisticsUpdateResult)
    {
        Debug.Log("Stats updated.");
    }

    private void OnStatsUpdateFailure(PlayFabError playFabError)
    {
        Debug.Log("Failed to update stats.");
    }

    #endregion

    #region PlayerLeaderBoard

    public void GetPlayerLeaderBoard( string statisticName, int maxEntries)
    {
        GetLeaderboardAroundPlayerRequest playerLeaderboardRequest = new GetLeaderboardAroundPlayerRequest { StatisticName = statisticName, MaxResultsCount = maxEntries };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(playerLeaderboardRequest, OnLeaderBoardSuccess, OnLeaderBoardFailure);
    }

    public delegate void OnLeaderboardUpdate(List<PlayerLeaderboardEntry> entries);
    public OnLeaderboardUpdate OnLeaderboardUpdated;

    private void OnLeaderBoardSuccess(GetLeaderboardAroundPlayerResult playerLeaderboardResult)
    {
        OnLeaderboardUpdated?.Invoke(playerLeaderboardResult.Leaderboard);
    }

    private void OnLeaderBoardFailure(PlayFabError playFabError)
    {
        Debug.Log("Failed to fetch leaderboard.");
    }

    #endregion
}
