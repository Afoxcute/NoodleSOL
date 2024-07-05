using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawManager : NetworkBehaviour
{
    [SerializeField] private Line _linePrefab;
    public const float RESOLUTION = 0.1f;

    private Camera _cam;

    [SyncVar] private Line _currentLine;
    private Line _currentLineSinglePlayer;

    [SyncVar] private List<string> _itemsMatched = new();
    //  private List<string> _itemsMatchedCl = new();

    private List<Transform> _itemsMatchedSinglePlayer = new();

    private NetworkManagerNoodlePair networkManagerNoodlePair;

    private Transform _currentItemBeingMatchedTransform;

    void Start()
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            networkManagerNoodlePair = NetworkManager.singleton as NetworkManagerNoodlePair;
        }

        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            HandleMouseInput();
        }
        else if (isOwned && !PreventGameInteraction())
        {
            MultiplayerHandleMouseInput();
        }
    }

    private bool PreventGameInteraction()
    {
        foreach (NetworkGamePlayerNoodlePair gamePlayer in networkManagerNoodlePair.GamePlayers)
        {
            if (gamePlayer.PreventGameInteraction() == true)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleMouseInput()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position into the scene
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Check if the ray hits any collider
            if (hit.collider != null)
            {
                // Check if the hit collider has the "ItemToMatch" tag
                if (hit.collider.CompareTag("ItemToMatch"))
                {
                    bool containsTransform = _itemsMatchedSinglePlayer.Contains(hit.transform);

                    if (containsTransform)
                    {
                        return;
                    }

                    MainSceneManager.Instance.CurrentItemBeingMatched = hit.transform;

                    _currentLineSinglePlayer = Instantiate(_linePrefab, mousePos, Quaternion.identity);
                    SetLineColor(_currentLineSinglePlayer);
                }
            }
        }

        if (Input.GetMouseButton(0) && _currentLineSinglePlayer != null && !_currentLineSinglePlayer.LineAtDestination)
        {
            AddPositionToLine(_currentLineSinglePlayer.gameObject, mousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentLineSinglePlayer == null)
            {
                return;
            }

            if (!_currentLineSinglePlayer.LineAtDestination)
            {
                Destroy(_currentLineSinglePlayer.gameObject);
            }
            else
            {
                _currentLineSinglePlayer = null;
            }
        }
    }

    [Client]
    private void MultiplayerHandleMouseInput()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position into the scene
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Check if the ray hits any collider
            if (hit.collider != null)
            {
                // Check if the hit collider has the "ItemToMatch" tag
                if (hit.collider.CompareTag("ItemToMatch"))
                {
                    bool containsTransform = _itemsMatched.Contains(hit.transform.name);

                    if (containsTransform)
                    {
                        return;
                    }

                    _currentItemBeingMatchedTransform = hit.transform;

                    // Call a command on the server to spawn the _currentLine.
                    CmdSpawnLine(mousePos);
                }
            }
        }

        if (Input.GetMouseButton(0) && _currentLine != null && !_currentLine.LineAtDestination)
        {
            CmdCallSetCurrentItemBeingMatchedFromSever(_currentItemBeingMatchedTransform.name);

            AddPositionToLine(_currentLine.gameObject, mousePos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentLine == null)
            {
                return;
            }

            if (!_currentLine.LineAtDestination)
            {
                // Destroy the _currentLine on the server (and synchronize it with clients).
                CmdDestroyCurrentLine(_currentLine.gameObject);
            }
            else
            {
                _currentLine = null;
            }

            foreach (NetworkGamePlayerNoodlePair gamePlayer in networkManagerNoodlePair.GamePlayers)
            {
                gamePlayer.WaitYourTurnActive("");
            }
        }
    }

    public void WaitYourTurnActiveInterface(string reasonText)
    {
        if (isOwned)
        {
            WaitYourTurnActive(reasonText);
        }
    }

    [Client]
    private void WaitYourTurnActive(string reasonText)
    {
        foreach (NetworkGamePlayerNoodlePair gamePlayer in networkManagerNoodlePair.GamePlayers)
        {
            gamePlayer.WaitYourTurnActive(reasonText);
        }
    }

    [Command]
    private void CmdDestroyCurrentLine(GameObject lineToDestroy)
    {
        NetworkServer.Destroy(lineToDestroy);
    }

    [Command]
    private void CmdSpawnLine(Vector2 spawnPosition)
    {
        // Register the prefab with the client
        NetworkClient.RegisterPrefab(_linePrefab.gameObject);

        // Spawn the prefab on the client-side
        Line tempLine = Instantiate(_linePrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(tempLine.gameObject, connectionToClient);
        RpcSetLineColors(tempLine);

        _currentLine = tempLine;
    }

    [ClientRpc]
    private void RpcSetLineColors(Line tempLine)
    {
        SetLineColor(tempLine);
    }

    [Command]
    private void CmdCallSetCurrentItemBeingMatchedFromSever(string hitTransformName)
    {
        //set the item to be matched on all instances
        RpcSetMainSceneManagerCurrentItemBeingMatched(hitTransformName);
    }

    private void AddPositionToLine(GameObject lineObject, Vector2 position)
    {
        if (lineObject.TryGetComponent<Line>(out Line line))
        {

            if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
            {
                line.AddPosition(position, MainSceneManager.Instance.LineColor);
            }
            else
            {
                line.CmdAddPosition(position, MainSceneManager.Instance.LineColor);
            }
        }
    }

    [ClientRpc]
    void RpcSetMainSceneManagerCurrentItemBeingMatched(string hitTransformName)
    {
        Transform hitTransform = GameObject.Find(hitTransformName).transform;

        MainSceneManager.Instance.CurrentItemBeingMatched = hitTransform;
    }

    public void SetItemsMatchedClientInterface(Transform item1, Transform item2)
    {
        SetItemsMatchedClient(item1.name, item2.name);
    }

    [Client]
    private void SetItemsMatchedClient(string item1, string item2)
    {
        if (!isOwned)
        {
            return;
        }

        CmdSetItemsMatched(item1, item2);
    }

    [Command]
    private void CmdSetItemsMatched(string item1, string item2)
    {
        _itemsMatched.Add(item1);
        _itemsMatched.Add(item2);

        RpcSetItemsMatched(_itemsMatched);
    }

    [ClientRpc]
    private void RpcSetItemsMatched(List<string> itemsMatched)
    {
        _itemsMatched = itemsMatched;
    }

    public void SetItemsMatched(Transform item1, Transform item2)
    {
        _itemsMatchedSinglePlayer.Add(item1);
        _itemsMatchedSinglePlayer.Add(item2);
    }

    public void SetLineColor(Line line)
    {
        // Get the LineRenderer component from the GameObject
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

        // Create a Gradient with the modified color
        Gradient gradient = new();
        gradient.SetKeys(new GradientColorKey[] { new(MainSceneManager.Instance.LineColor, 0f) }, new GradientAlphaKey[] { new(1f, 0f) });

        // Assign the gradient to the LineRenderer
        lineRenderer.colorGradient = gradient;

    }
}
