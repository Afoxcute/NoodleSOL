using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MainSceneManager : MonoBehaviour
{
    private static MainSceneManager _instance;

    public static MainSceneManager Instance
    {
        get
        {
            // If the instance doesn't exist, find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<MainSceneManager>();

                // If there is no instance in the scene, create a new GameObject and add the script
                if (_instance == null)
                {
                    GameObject singletonObject = new(typeof(MainSceneManager).Name);
                    _instance = singletonObject.AddComponent<MainSceneManager>();
                }
            }

            return _instance;
        }
    }

    [SerializeField] private DrawManager _drawManager;

    [SerializeField] private AudioClip _buttonClick;

    [SerializeField] private GameObject _menuPanel, _selectModePanel, _selectItemToMatchPanel, _selectColorPanel;

    [SerializeField] private GameObject _goodJobSection;
    [SerializeField] private GameObject _tryAgainSection;
    [SerializeField] private GameObject _multiplayerTieSection;
    [SerializeField] private GameObject _multiplayerGameDoneSection;

    [Header("TwoPlayersActionButtons")]
    [SerializeField] private GameObject _gameDonePlayAgainButton;
    [SerializeField] private GameObject _gameDoneAnotherSetButton;
    [SerializeField] private GameObject _tiePlayAgainButton;
    [SerializeField] private GameObject _tieAnotherSetButton;

    // [Header("Multiplayer")]
    // [SerializeField] private GameObject _panelStart;
    // [SerializeField] private GameObject _panelStop;

    // [SerializeField] private TMP_InputField _inputFieldAddressClient;

    // [SerializeField] private TextMeshProUGUI _serverText;
    // [SerializeField] private TextMeshProUGUI _clientText;

    private Color _lineColor;
    public Color LineColor => _lineColor;

    public Transform CurrentItemBeingMatched { get; set; }

    private void Awake()
    {
        // Ensure there is only one instance of this singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Set this instance as the singleton instance if it doesn't exist
            _instance = this;
        }
    }


    private void Start()
    {
        // Check if this is the first time loading the menu scene on game launch
        if (!PlayerPrefs.HasKey("MenuSceneLoaded"))
        {
            // This is the first time loading the menu scene, do first-time setup.
            PlayerPrefs.SetInt("MenuSceneLoaded", 1);
        }
        else if (SceneManager.GetActiveScene().name == "Menu")
        {
            // Make sure you're currently in the "Menu" before showing the select mode panel.
            _menuPanel.SetActive(false);

            FindObjectOfType<SelectModePanel>(true).gameObject.SetActive(true);
        }

        // // stop host if host mode
        // if (NetworkServer.active && NetworkClient.isConnected)
        // {
        //     _gameDonePlayAgainButton.SetActive(true);
        //     _gameDoneAnotherSetButton.SetActive(true);
        //     _tiePlayAgainButton.SetActive(true);
        //     _tieAnotherSetButton.SetActive(true);

        // }
        // // stop client if client-only
        // else if (NetworkClient.isConnected)
        // {
        //     _gameDonePlayAgainButton.SetActive(false);
        //     _gameDoneAnotherSetButton.SetActive(false);
        //     _tiePlayAgainButton.SetActive(false);
        //     _tieAnotherSetButton.SetActive(false);
        // }

        if (PlayerPrefs.HasKey("ColorName"))
        {
            // Retrieve the color string from PlayerPrefs
            string colorHex = PlayerPrefs.GetString("ColorName", "");

            if (!string.IsNullOrEmpty(colorHex))
            {
                // Try to convert the string back to a color
                if (ColorUtility.TryParseHtmlString("#" + colorHex, out Color loadedColor))
                {
                    _lineColor = loadedColor;
                }
                else
                {
                    // Handle the case where parsing the color fails
                    Debug.LogError("Failed to parse the color from PlayerPrefs.");
                }
            }
            else
            {
                // Handle the case where the PlayerPrefs key doesn't exist
                Debug.LogWarning("No color found in PlayerPrefs.");
            }
        }

        // if (SceneManager.GetActiveScene().name == "Main")
        // {
        //     Debug.Log("Calling level to active");
        //     GameObject.Find("Levels").transform.GetChild(0).gameObject.SetActive(true);
        // }

        //     //Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
        //     if (NetworkManager.singleton.networkAddress != "localhost") { _inputFieldAddressClient.text = NetworkManager.singleton.networkAddress; }

        //     //Adds a listener to the main input field and invokes a method when the value changes.
        //     _inputFieldAddressClient.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        //     // buttonStop.onClick.AddListener(ButtonStop);

        //     //This updates the Unity canvas, we have to manually call it every change, unlike legacy OnGUI.
        //     SetupCanvas();
    }

    private void OnApplicationQuit()
    {
        // Clear the PlayerPrefs key when the game is closed.
        PlayerPrefs.DeleteKey("MenuSceneLoaded");
    }

    // // Invoked when the value of the text field changes.
    // public void ValueChangeCheck()
    // {
    //     NetworkManager.singleton.networkAddress = _inputFieldAddressClient.text;
    // }

    public void LoadMenuScene()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NetworkManagerNoodlePair networkManager = NetworkManager.singleton as NetworkManagerNoodlePair;

            networkManager.ShutDownAndGoToMenuOnAnyDisconnect();

            // Unset the "Don't Destroy on Load" flag to allow for destruction
            DontDestroyOnLoad(networkManager.gameObject);

            // Destroy the NetworkManagerNoodlePair
            Destroy(networkManager.gameObject);

            Debug.Log("Destroyed NetworkManagerNoodlePair.");
        }
    }

    public void DestroyNetworkManager()
    {
        // // Find the NetworkManagerNoodlePair
        // NetworkManagerNoodlePair networkManager = FindObjectOfType<NetworkManagerNoodlePair>();

        // if (networkManager != null)
        // {
        //     Debug.Log("Found NetworkManagerNoodlePair: " + networkManager.gameObject.name);

        //     // Unset the "Don't Destroy on Load" flag to allow for destruction
        //     DontDestroyOnLoad(networkManager.gameObject);

        //     // Destroy the NetworkManagerNoodlePair
        //     Destroy(networkManager.gameObject);

        //     Debug.Log("Destroyed NetworkManagerNoodlePair.");
        // }
        // else
        // {
        //     Debug.Log("NetworkManagerNoodlePair not found in the current scene.");
        // }
    }

    public void ButtonHost()
    {
        NetworkManager.singleton.StartHost();
        //  SetupCanvas();
    }

    public void ButtonServer()
    {
        NetworkManager.singleton.StartServer();
        //   SetupCanvas();
    }

    public void ButtonClient()
    {
        NetworkManager.singleton.StartClient();
        //  SetupCanvas();
    }

    public void ButtonStop()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }

        //   SetupCanvas();
    }

    public void SkipSelectItemPanelBasedOnNetworkSelectColorPanel()
    {
        // if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            _selectItemToMatchPanel.SetActive(true);
        }
        // if client-only
        else if (NetworkClient.isConnected)
        {
            FindLobbyUIAndSetActive();//Client activates lobby panel
        }
        //if single player
        else
        {
            _selectItemToMatchPanel.SetActive(true);
        }
    }

    public void SkipSelectItemPanelBasedOnNetworkLobbyPanelBackButton()//Lobby Panel only multiplayer
    {
        // if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            _selectItemToMatchPanel.SetActive(true);
        }
        // if client-only
        else if (NetworkClient.isConnected)
        {
            FindObjectOfType<SelectColorPanel>(true).gameObject.SetActive(true);
        }
    }

    public void SetLevelOverNetworkAndActivateLobbyPanel(string level)
    {
        PlayerPrefs.SetString("LevelName", level);

        //if host mode or client mode
        if ((NetworkServer.active && NetworkClient.isConnected) || NetworkClient.isConnected)
        {
            FindLobbyUIAndSetActive();//Server activates lobby panel
        }
        else if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            // Find the NetworkManagerNoodlePair
            NetworkManagerNoodlePair networkManager = FindObjectOfType<NetworkManagerNoodlePair>();

            if (networkManager != null)
            {
                // Unset the "Don't Destroy on Load" flag to allow for destruction
                DontDestroyOnLoad(networkManager.gameObject);

                // Destroy the NetworkManagerNoodlePair
                Destroy(networkManager.gameObject);
            }
            else
            {
                Debug.LogError("NetworkManagerNoodlePair not found in the current scene.");
            }

            SceneManager.LoadScene(1);
        }
    }

    // public void SetupCanvas()
    // {
    //     // Here we will dump majority of the canvas UI that may be changed.

    //     if (!NetworkClient.isConnected && !NetworkServer.active)
    //     {
    //         if (NetworkClient.active)
    //         {
    //             // PanelStart.SetActive(false);
    //             // PanelStop.SetActive(true);
    //             _clientText.text = "Connecting to " + NetworkManager.singleton.networkAddress + "..";
    //         }
    //         else
    //         {
    //             // PanelStart.SetActive(true);
    //             // PanelStop.SetActive(false);
    //         }
    //     }
    //     else
    //     {
    //         // PanelStart.SetActive(false);
    //         // PanelStop.SetActive(true);

    //         // server / client status message
    //         if (NetworkServer.active)
    //         {
    //             _serverText.text = "Server: active. Transport: " + Transport.active;
    //             // Note, older mirror versions use: Transport.activeTransport
    //         }
    //         if (NetworkClient.isConnected)
    //         {
    //             _clientText.text = "Client: address=" + NetworkManager.singleton.networkAddress;
    //         }
    //     }
    // }

    public void ChangeView(GameObject currentView, GameObject nextView)
    {
        currentView.SetActive(false);
        nextView.SetActive(true);
    }

    public void PlayButtonClick()
    {
        AudioSource.PlayClipAtPoint(_buttonClick, Camera.main.transform.position);
    }

    public void AboutGameBackButtonReset()
    {
        // Find the GameObject with the name "Container"
        GameObject container = GameObject.Find("Container");

        // Check if the GameObject was found
        if (container != null)
        {
            // Check if the RectTransform component was found
            if (container.TryGetComponent<RectTransform>(out var containerRectTransform))
            {
                // Set the x position of the RectTransform
                containerRectTransform.anchoredPosition = new Vector2(1055.542f, containerRectTransform.anchoredPosition.y);
            }
            else
            {
                Debug.LogError("RectTransform component not found on the GameObject named 'Container'.");
            }
        }
        else
        {
            Debug.LogError("GameObject with the name 'Container' not found.");
        }
    }

    public void SetLineColor(TextMeshProUGUI colorText)
    {
        _lineColor = colorText.color;

        // Convert the color to a hexadecimal string and store it in PlayerPrefs
        string colorHex = ColorUtility.ToHtmlStringRGBA(_lineColor);
        PlayerPrefs.SetString("ColorName", colorHex);

        // Save the PlayerPrefs to make sure the data is stored
        PlayerPrefs.Save();
    }

    public void StoreGameMode(string gameMode)
    {
        PlayerPrefs.SetString("GameMode", gameMode);
    }

    public void TurnOnLevelUIChildren()
    {
        ToggleOnLevelUIChildren(true);
    }

    public void TriggerGoodJobSection()
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            return;
        }

        ToggleOnLevelUIChildren(false);
        _goodJobSection.SetActive(true);
    }

    public void TriggerTieSection()
    {
        ToggleOnLevelUIChildren(false);
        _multiplayerTieSection.SetActive(true);
    }

    public void TriggerMultiplayerGameDoneSection(string winnerNameText, string winnerScoreText, string otherPlayerNameText, string otherPlayerScoreText)
    {
        ToggleOnLevelUIChildren(false);
        _multiplayerGameDoneSection.SetActive(true);
        _multiplayerGameDoneSection.transform.Find("WinnerNameText").GetComponent<TextMeshProUGUI>().text = winnerNameText + "!";
        _multiplayerGameDoneSection.transform.Find("WinnerScoreText").GetComponent<TextMeshProUGUI>().text = winnerScoreText;
        _multiplayerGameDoneSection.transform.Find("OtherPlayerNameText").GetComponent<TextMeshProUGUI>().text = otherPlayerNameText;
        _multiplayerGameDoneSection.transform.Find("OtherPlayerScoreText").GetComponent<TextMeshProUGUI>().text = otherPlayerScoreText;
    }

    public void TriggerTryAgainSection(string tryAgainText)
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            return;
        }

        ToggleOnLevelUIChildren(false);
        _tryAgainSection.SetActive(true);
        _tryAgainSection.transform.Find("ReasonText").GetComponent<TextMeshProUGUI>().text = tryAgainText;
    }

    public void ResetLevel()
    {
        FindObjectOfType<LevelUI>().NumberOfItemsMatched = 0;
        FindAndDeleteLines();
        FindObjectOfType<LevelUI>().RevertAlphasOfAllItemsToMatch();
    }

    public void TurOffCurrentLevel()//Another set button?
    {
        GameObject.FindGameObjectWithTag("Level").SetActive(false);
    }

    public void TurnOffCurrentLevelAndCategory()//menu button?
    {
        GameObject.FindGameObjectWithTag("Level").SetActive(false);//doesn't really matter cause prefab will be destroyed
        Destroy(FindObjectOfType<RandomLevelNotRecentlyPickedSelector>().gameObject);
    }

    public void ActivateMainAudioBG()//menu button
    {
        SwitchAudioBGState("GameAudioBG", false);
        SwitchAudioBGState("MainAudioBG", true);
    }

    private void SwitchAudioBGState(string tagName, bool state)
    {
        if (GameObject.FindGameObjectWithTag(tagName).TryGetComponent(out AudioSource audioSource))
        {
            audioSource.enabled = state;
        }
    }

    private void FindLobbyUIAndSetActive()
    {
        NetworkManagerNoodlePair Room = NetworkManager.singleton as NetworkManagerNoodlePair;

        foreach (var player in Room.RoomPlayers)
        {
            if (player.isOwned)
            {
                player.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private void FindAndDeleteLines()
    {
        Line[] lines = FindObjectsOfType<Line>();

        foreach (Line line in lines)
        {
            Destroy(line.gameObject);
        }
    }

    private void ToggleOnLevelUIChildren(bool state)
    {
        Transform currentActiveLevelUITransform = GameObject.Find("LevelUI").GetComponent<Transform>();

        // Loop through all immediate child GameObjects
        for (int i = 0; i < currentActiveLevelUITransform.childCount; i++)
        {
            // Get the child GameObject at index i
            GameObject child = currentActiveLevelUITransform.GetChild(i).gameObject;

            // Activate the child GameObject
            child.SetActive(state);
        }
    }
}
