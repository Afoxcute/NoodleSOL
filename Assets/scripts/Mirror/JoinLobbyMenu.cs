using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerNoodlePair networkManager = null;

    [Header("UI")]
    // [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        NetworkManagerNoodlePair.OnClientConnected += HandleClientConnected;
        NetworkManagerNoodlePair.OnClientDisconnected += HandleClientDisconnected;

        joinButton.interactable = false;
    }

    private void OnDisable()
    {
        NetworkManagerNoodlePair.OnClientConnected -= HandleClientConnected;
        NetworkManagerNoodlePair.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        FindObjectOfType<NetworkManagerNoodlePair>(true).networkAddress = ipAddress;
        FindObjectOfType<NetworkManagerNoodlePair>(true).StartClient();
    }

    public void SetIPAddress(string name)
    {
        joinButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void JoinButtonInteractable()
    {
        joinButton.interactable = !string.IsNullOrEmpty(ipAddressInputField.text);
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        // gameObject.SetActive(false);
        // landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
