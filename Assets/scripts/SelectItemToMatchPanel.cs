using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class SelectItemToMatchPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text networkAddressText;

    public void SetNetworkAddress(string ipAddress)
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            networkAddressText.gameObject.SetActive(true);
            networkAddressText.text = ipAddress;
        }
    }
}
