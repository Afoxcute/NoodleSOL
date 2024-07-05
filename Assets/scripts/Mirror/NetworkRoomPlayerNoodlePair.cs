using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class NetworkRoomPlayerNoodlePair : NetworkBehaviour
{
    [Header("UI")]
    // [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private TMP_Text networkAddressText;

    [Header("Levels")]
    [SerializeField] private GameObject _lettersLevelPrefab;
    [SerializeField] private GameObject _numbersLevelPrefab;
    [SerializeField] private GameObject _animalsLevelPrefab;
    [SerializeField] private GameObject _colorsLevelPrefab;
    [SerializeField] private GameObject _fruitsLevelPrefab;
    [SerializeField] private GameObject _birdsLevelPrefab;
    [SerializeField] private GameObject _insectsLevelPrefab;
    [SerializeField] private GameObject _vehiclesLevelPrefab;
    [SerializeField] private GameObject _environmentLevelPrefab;

    public string ipAddress { get; private set; }

    //SYNC VARS ARE VARIABLES THAT CAN ONLY BE CHANGED ON THE SERVER
    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
            networkAddressText.gameObject.SetActive(value);
        }
    }

    private NetworkManagerNoodlePair room;
    private NetworkManagerNoodlePair Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerNoodlePair;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        startGameButton.interactable = false;

        // Get the local IP address of the device
        string hostName = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = address.ToString();
                break;
            }
        }

        // Display the IP address
        Debug.Log("Server IP Address: " + ipAddress);

        networkAddressText.text = ipAddress;

        FindObjectOfType<SelectColorPanel>(true).SetNetworkAddress(ipAddress);
        FindObjectOfType<SelectItemToMatchPanel>(true).SetNetworkAddress(ipAddress);
    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);

        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!isOwned)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.isOwned)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        Room.StartGame();
        // SwitchAudioBGState("MainAudioBG", false);
        //  SwitchAudioBGState("GameAudioBG", true);
    }

    private void SwitchAudioBGState(string tagName, bool state)
    {
        if (GameObject.FindGameObjectWithTag(tagName).TryGetComponent(out AudioSource audioSource))
        {
            audioSource.enabled = state;
        }
    }

    [SerializeField] private AudioClip _buttonClick;

    public void PlayButtonClick()
    {
        AudioSource.PlayClipAtPoint(_buttonClick, Camera.main.transform.position);
    }

    public void SkipSelectItemPanelBasedOnNetworkLobbyPanelBackButton()//Lobby Panel only multiplayer
    {
        // if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            FindObjectOfType<SelectItemToMatchPanel>(true).gameObject.SetActive(true);
        }
        // if client-only
        else if (NetworkClient.isConnected)
        {
            FindObjectOfType<SelectColorPanel>(true).gameObject.SetActive(true);
        }
    }


    // [SyncVar(hook = nameof(HandlePlayerColorChange))]
    // public Color PlayerColor;
    // [SyncVar(hook = nameof(HandlePlayerScoreChange))]
    // public int PlayerScore;

    // public void HandlePlayerColorChange(bool oldValue, bool newValue) => UpdateMyColorOnEveryOtherPlayer();
    // public void HandlePlayerScoreChange(string oldValue, string newValue) => UpdateMyScoreOnEveryOtherPlayer();


    // private void UpdateMyColorOnEveryOtherPlayer()
    // {

    // }

    // private void UpdateMyScoreOnEveryOtherPlayer()
    // {

    // }

    // [Command]
    // private void CmdSetPlayerColor()
    // {
    //     PlayerColor = MainSceneManager.Instance.LineColor;
    // }

    // [Command]
    // private void CmdSetPlayerScore()
    // {
    //     PlayerScore = FindObjectOfType<LevelUI>().NumberOfItemsMatched;
    // }
}
