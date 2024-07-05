using UnityEngine;
using TMPro;
using Mirror;

public class NetworkGamePlayerNoodlePair : NetworkBehaviour
{
    [SyncVar]
    private string displayName = "Loading...";
    [SyncVar]
    private int playerScore;

    public GameObject waitYourTurnSection;

    public bool preventGameInteraction;

    private NetworkManagerNoodlePair room;
    private NetworkManagerNoodlePair Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerNoodlePair;
        }
    }

    public string PlayerName => displayName;

    public int PlayerScore => playerScore;

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void IncrementPlayerScore()
    {
        this.playerScore++;
    }

    [ClientRpc]
    public void RpcWaitYourTurnActive()
    {
        WaitYourTurnActive("");
    }

    public void WaitYourTurnActive(string reasonText)
    {
        if (isOwned)
        {
            waitYourTurnSection.SetActive(true);
            waitYourTurnSection.transform.Find("ReasonText").GetComponent<TextMeshProUGUI>().text = reasonText;
            preventGameInteraction = true;
            CmdTurnOffWaitYourTurnOthers();
        }
    }

    [Command]
    private void CmdTurnOffWaitYourTurnOthers()
    {
        RpcTurnOffWaitYourTurnOthers();
    }

    [ClientRpc]
    private void RpcTurnOffWaitYourTurnOthers()
    {
        if (!isLocalPlayer) // Check if this is not the local player
        {
            // Find all objects of type NetworkGamePlayerNoodlePair
            NetworkGamePlayerNoodlePair[] gamePlayerObjects = FindObjectsOfType<NetworkGamePlayerNoodlePair>();

            // Loop through the found player objects
            foreach (NetworkGamePlayerNoodlePair gamePlayerObject in gamePlayerObjects)
            {
                // Check if the current game player object is not equal to the script running the loop
                if (gamePlayerObject != this)
                {
                    gamePlayerObject.waitYourTurnSection.SetActive(false);
                    gamePlayerObject.preventGameInteraction = false;
                }
            }
        }
    }

    public bool PreventGameInteraction()
    {
        if (isOwned)
        {
            return preventGameInteraction;
        }

        return false;
    }
}
