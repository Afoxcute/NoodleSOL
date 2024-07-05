using UnityEngine;

public class MultiplayerMainMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerNoodlePair networkManager = null;

    // [Header("UI")]
    // [SerializeField] private GameObject landingPagePanel = null;

    public void HostLobby()
    {
        FindObjectOfType<NetworkManagerNoodlePair>(true).StartHost();

        // landingPagePanel.SetActive(false);
    }
}
