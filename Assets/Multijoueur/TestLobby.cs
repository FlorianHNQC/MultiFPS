using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.FPS.Game;

public class TestLobby : NetworkBehaviour
{
    [SerializeField] private NetworkManager m_NetworkManager;

    [SerializeField] private GameObject listLobbyPanel;
    [SerializeField] private TMP_InputField inputLobbyCode;
    [SerializeField] private Button submitCode;
    [SerializeField] private GameObject lobbyCamera;

    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private Button createLobby;
    [SerializeField] private Button mapLobby;
    [SerializeField] private TMP_InputField playerNameLobby;
    [SerializeField] private Button gameModeLobby;

    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_Text lobbyCode;
    [SerializeField] private TMP_Text lobbyNbPlayers;

    [SerializeField] private Button createLobbyButton;

    [SerializeField] private Button startGameButton;

    public string sceneToLoad = "TestMenu";
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;
    private int nbPlayers;
    private bool isHostLobby = false;
    private List<Actor> actors = new List<Actor>();

    private void Awake()
    {
        createLobby.onClick.AddListener(() =>
        {
            string text = playerNameLobby.textComponent.text;
            if (playerNameLobby.textComponent.text != null) CreateLobby(playerNameLobby.textComponent.text);
            else CreateLobby();
            if (m_NetworkManager.StartHost() == true) EnableDisableActors(false);
            else Debug.Log("Server can't start");
        });
        if (inputLobbyCode.textComponent.text != null) {
            submitCode.onClick.AddListener(() =>
            {
                JoinLobbyByCode(inputLobbyCode.textComponent.text);
                listLobbyPanel.SetActive(false);
                createLobbyPanel.SetActive(false);
                lobbyPanel.SetActive(true);
                m_NetworkManager.StartClient();
            });
        }
        createLobbyButton.onClick.AddListener(() =>
        {
            listLobbyPanel.SetActive(false);
            createLobbyPanel.SetActive(true);
            lobbyPanel.SetActive(false);
        });
        startGameButton.onClick.AddListener(() =>
        {
            StartGame();
        });
    }

    private void disableCamera()
    {
        lobbyCamera.SetActive(false);
    }

    private async void Start()
    {
        listLobbyPanel.SetActive(true);
        createLobbyPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Fps" + UnityEngine.Random.Range(1, 10);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        UpdateLobbyPrintedData();
        int currentNbPlayers = GetNbPlayer();
        if (nbPlayers != currentNbPlayers) {
            Debug.Log("Nb players changed");
            EnableDisableActors(false);
            nbPlayers = currentNbPlayers;
        }
        if (Input.GetKeyDown(KeyCode.Return) && lobbyPanel.activeSelf)
        {
            StartGame();
        }
    }

    private void UpdateLobbyPrintedData()
    {
        if (joinedLobby == null) return;
        lobbyCode.text = joinedLobby.LobbyCode;
        lobbyNbPlayers.text = "Nb Players: " + GetNbPlayer().ToString() + "/" + joinedLobby.MaxPlayers.ToString();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                Debug.Log("Heartbeat timer");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            }
        }
    }

    private void EnableDisableActors(bool todo)
    {
        if (!todo)
        {
            if (actors == null) actors = new List<Actor>();
            Actor[] actorComponents = FindObjectsOfType<Actor>();
            foreach (Actor actor in actorComponents)
            {
                actors.Add(actor);
                actor.gameObject.SetActive(todo);
            }
        }

        else if (todo)
        {
            foreach (Actor actor in actors) actor.gameObject.SetActive(todo);
        }
/*        if (actors.Length > 0)
            foreach (Actor actor in actors) actor.gameObject.SetActive(todo);
        else {
            actors = FindObjectsOfType<Actor>();
            if (actors.Length > 0)
                foreach (Actor actor in actors) actor.gameObject.SetActive(todo);
        }*/
    }

    private void StartGame()
    {
        Debug.Log("Start game");
         //m_NetworkManager.StartHost();
        if (IsServer)
            EnableDisableActors(true);
        if (m_NetworkManager.IsServer)
        {
            disableCamera();
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Only the server can change the scene");
        }
    }

    [ClientRpc]
    public void InitPlayersClientRpc(bool todo)
    {
        EnableDisableActors(todo);
    }

    private void CreateLobby()
    {
        CreateLobby(playerName);
    }

    private async void CreateLobby(string playernameLobby)
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") }, //Deathmatch, BR, CaptureTheFlag
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Mapname") } //update scene name
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log("Created " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode + joinedLobby.Data["GameMode"].Value);
            listLobbyPanel.SetActive(false);
            createLobbyPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            isHostLobby = true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby result in queryResponse.Results)
            {
                Debug.Log(result.Name + " " + result.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        lobbyCode = lobbyCode.Substring(0, 6);
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Lobby found & joined: " + lobbyCode);

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
        return player;
    }

    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name + "\nMode: " + lobby.Data["GameMode"].Value + "\nMap: " + lobby.Data["Map"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private int GetNbPlayer()
    {
        return GetNbPlayer(joinedLobby);
    }

    private int GetNbPlayer(Lobby lobby)
    {
        if (joinedLobby == null) return 0;
        int nbPlayer = 0;
        foreach (Player player in lobby.Players)
        {
            nbPlayer++;
        }
        return nbPlayer;
    }
}
