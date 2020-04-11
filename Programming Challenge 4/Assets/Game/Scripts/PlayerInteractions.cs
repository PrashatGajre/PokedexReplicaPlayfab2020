using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Pokemon UI")]
    [SerializeField] InputField pokemonIdInput;
    [SerializeField] PokeDex pokeDex;

    [Header("Player UI")]
    [SerializeField] Text welcomeText;
    [SerializeField] Toggle updateNameToggle;
    [SerializeField] InputField updatePlayerNameInput;
    [SerializeField] Button updatePlayerButton;
    [SerializeField] Transform leaderboardContent;

    int selectedPokemonId = 0;
    int pokemonCaught = 0;
    private void Awake()
    {
        PlayFabManager.Instance.OnPlayerLoggedIn += UpdatePlayerNameUI;
        PlayFabManager.Instance.OnPlayerNameUpdated += UpdatePlayerNameUI;
        PlayFabManager.Instance.OnStatsUpdated += FetchPlayerStat;
        PlayFabManager.Instance.OnLeaderboardUpdated += UpdateLeaderboard;
    }

    void Start()
    {
        Input.location.Start();
        if (pokeDex == null)
        {
            pokeDex = GameObject.FindObjectOfType<PokeDex>();
        }
    }
    private void OnDestroy()
    {
        PlayFabManager.Instance.OnPlayerLoggedIn -= UpdatePlayerNameUI;
        PlayFabManager.Instance.OnPlayerNameUpdated -= UpdatePlayerNameUI;
        PlayFabManager.Instance.OnStatsUpdated -= FetchPlayerStat;
        PlayFabManager.Instance.OnLeaderboardUpdated -= UpdateLeaderboard;
    }

    public void UpdatePlayerNameUI()
    {
        welcomeText.text = "Welcome " + PlayFabManager.Instance.UserName;
        updatePlayerNameInput.text = PlayFabManager.Instance.UserName;
        PlayFabManager.Instance.GetPlayerStats(new List<string> { "num_pokemon_caught" });
    }

    public void FetchPlayerStat(List<PlayFab.ClientModels.StatisticValue>statistics)
    {
        foreach (PlayFab.ClientModels.StatisticValue stat in statistics)
        {
            if (stat.StatisticName == "num_pokemon_caught")
            {
                pokemonCaught = stat.Value;
                Debug.Log("Pokemon Caught = " + pokemonCaught);
            }
        }
    }

    public void FetchLeaderBoard()
    {
        PlayFabManager.Instance.GetPlayerLeaderBoard("num_pokemon_caught", 5);
    }

    public void UpdateLeaderboard(List<PlayFab.ClientModels.PlayerLeaderboardEntry> entries)
    {
        for (int i = leaderboardContent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(leaderboardContent.GetChild(i).gameObject);
        }

        foreach (PlayFab.ClientModels.PlayerLeaderboardEntry entry in entries)
        {
            string display = " <" + entry.Position.ToString("D2") + ">   " + string.Format("{0,-25}", string.Format("{0," + ((25 + entry.DisplayName.Length) / 2).ToString() + "}", entry.DisplayName)) + "      " + entry.StatValue.ToString("D2");
            Text text = GameObject.Instantiate(new GameObject(entry.DisplayName, typeof(RectTransform), typeof(Text)), leaderboardContent).GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 36;
            text.supportRichText = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 36;
            text.text = display;
        }
    }

    public void FetchNewPokemon()
    {
        int.TryParse(pokemonIdInput.text, out selectedPokemonId);
        if (selectedPokemonId < 1 || selectedPokemonId > 807)
        {
            int prev = selectedPokemonId;
            selectedPokemonId = Random.Range(1, 808);
            ShowAndroidToastMessage("Pokemon with id " + prev + " does not exist.\nFetching random Pokemon with id " + selectedPokemonId + ".");
        }
        pokeDex.FetchNewPokemon(selectedPokemonId);
    }

    private void ShowAndroidToastMessage(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#elif UNITY_ANDROID
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
#else
        Debug.Log(message);
#endif
    }

    public void CatchPokemon()
    {
        if (selectedPokemonId != 0)
        {
            pokemonCaught++;
            PlayFabManager.Instance.UpdatePlayerStatistics(new List<PlayFab.ClientModels.StatisticUpdate> { new PlayFab.ClientModels.StatisticUpdate { StatisticName = "num_pokemon_caught", Value = pokemonCaught } });
            PlayFabManager.Instance.LogPlayerEvent("caught_pokemon", new Dictionary<string, object> { { "name", pokeDex.pokemon.name }, { "location" , Input.location.lastData.latitude + ", " + Input.location.lastData.longitude } });
            ShowAndroidToastMessage("You caught your number " + pokemonCaught + " pokemon : " + pokeDex.pokemon.name);
        }
    }

    #region Validation
    public void UpdateToggleValidation()
    {
        if (updateNameToggle.isOn)
        {
            updatePlayerNameInput.interactable = true;
        }
    }

    public void UpdateNameValidation()
    {
        if (string.IsNullOrEmpty(updatePlayerNameInput.text) || string.IsNullOrWhiteSpace(updatePlayerNameInput.text) || !updateNameToggle.isOn)
        {
            updatePlayerButton.interactable = false;
        }
        else
        { 
            updatePlayerButton.interactable = true;
        }
    }

    public void UpdatePlayerDisplayName()
    {
        PlayFabManager.Instance.UpdateUserDisplayName(updatePlayerNameInput.text);
    }
    #endregion

    public void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}