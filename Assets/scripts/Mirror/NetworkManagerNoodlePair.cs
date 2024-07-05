// using DapperDino.Tutorials.Lobby;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerNoodlePair : NetworkManager
{
    [Header("Scene")]
    [SerializeField] private GameObject _selectModePanel;
    [SerializeField] private GameObject _selectColorPanel;

    [Header("Networking")]
    [SerializeField] private int minPlayers = 2;
    [Scene][SerializeField] private string menuScene = string.Empty;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerNoodlePair roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerNoodlePair gamePlayerPrefab = null;
    [SerializeField] private List<GameObject> levelSpawnSystems = new();
    [SerializeField] private GameObject drawManager;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public List<NetworkRoomPlayerNoodlePair> RoomPlayers { get; } = new();
    public List<NetworkGamePlayerNoodlePair> GamePlayers { get; } = new();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    // public override void OnStartClient() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();

        MoveToColorSelectionOnSuccessfulConnection();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();

        ShutDownAndGoToMenuOnAnyDisconnect();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().path != menuScene)//Stop players from joining when game in progress
        {
            conn.Disconnect();
            return;
        }

        //if first client
        if (RoomPlayers.Count == 0)
        {
            MoveToColorSelectionOnSuccessfulConnection();
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerNoodlePair roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerNoodlePair>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        // OnServerStopped?.Invoke();

        RoomPlayers.Clear();
        GamePlayers.Clear();

        //GoToSelectModeOnLostConnection();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private void MoveToColorSelectionOnSuccessfulConnection()
    {
        FindObjectOfType<SelectModePanel>(true).gameObject.SetActive(false);
        FindObjectOfType<SelectColorPanel>(true).gameObject.SetActive(true);
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }

            // mapHandler = new MapHandler(mapSet, numberOfRounds);

            ServerChangeScene("Main");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("Mai"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);

                if (i == 1)
                {
                    gameplayerInstance.RpcWaitYourTurnActive();
                }
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith("Mai"))
        {
            GameObject levelSpawnSystemInstance = null;

            switch (PlayerPrefs.GetString("LevelName"))
            {
                case "Letters":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[0]);
                    break;
                case "Numbers":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[1]);
                    break;
                case "Animals":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[2]);
                    break;
                case "Colors":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[3]);
                    break;
                case "Fruits":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[4]);
                    break;
                case "Birds":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[5]);
                    break;
                case "Insects":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[6]);
                    break;
                case "Vehicles":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[7]);
                    break;
                case "Environment":
                    levelSpawnSystemInstance = Instantiate(levelSpawnSystems[8]);
                    break;
            }

            NetworkServer.Spawn(levelSpawnSystemInstance);

            // Instantiate a separate drawManager for each player
            foreach (var player in GamePlayers)
            {
                GameObject drawManagerInstance = Instantiate(drawManager);
                NetworkServer.Spawn(drawManagerInstance, player.connectionToClient);
            }

            // GameObject roundSystemInstance = Instantiate(roundSystem);
            // NetworkServer.Spawn(roundSystemInstance);
        }
    }

    // public override void OnServerReady(NetworkConnection conn)
    // {
    //     base.OnServerReady(conn);

    //     OnServerReadied?.Invoke(conn);
    // }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

    public void ShutDownAndGoToMenuOnAnyDisconnect()
    {
        ServerChangeScene("Menu");
        MainSceneManager.Instance.ButtonStop();
    }
}
