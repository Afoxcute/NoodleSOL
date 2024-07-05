using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Line : NetworkBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private EdgeCollider2D _edgeCollider;
    [SerializeField] private Transform _circleColliderTransform;
    [SerializeField][Range(0.2f, 0.7f)] private float _lightUpAtDestinationBy = 0.6f;

    [SyncVar] private List<Vector2> _points = new List<Vector2>();
    private List<Vector2> _pointsSinglePlayer = new List<Vector2>();

    private NetworkManagerNoodlePair networkManagerNoodlePair;

    private Color _lineColor;

    public bool LineAtDestination { get; set; }

    private void Start()
    {
        networkManagerNoodlePair = NetworkManager.singleton as NetworkManagerNoodlePair;
        _edgeCollider.transform.position -= transform.position;
    }

    [Command]
    public void CmdAddPosition(Vector2 pos, Color lineColor)
    {
        if (!CanAppend(pos)) return;

        _points.Add(pos);

        RpcUpdatePositions(_points, pos, lineColor);
    }

    public void AddPosition(Vector2 pos, Color lineColor)
    {
        if (!CanAppend(pos)) return;

        _pointsSinglePlayer.Add(pos);

        UpdatePositions(_pointsSinglePlayer, pos, lineColor);
    }


    [ClientRpc]
    void RpcUpdatePositions(List<Vector2> positions, Vector2 position, Color lineColor)
    {
        _points = positions;
        _lineColor = lineColor;

        _lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            _lineRenderer.SetPosition(i, positions[i]);
        }

        _circleColliderTransform.position = position;

        _edgeCollider.points = _points.ToArray();
    }

    void UpdatePositions(List<Vector2> positions, Vector2 position, Color lineColor)
    {
        _pointsSinglePlayer = positions;
        _lineColor = lineColor;

        _lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            _lineRenderer.SetPosition(i, positions[i]);
        }

        _circleColliderTransform.position = position;

        _edgeCollider.points = _pointsSinglePlayer.ToArray();
    }

    private bool CanAppend(Vector2 pos)
    {
        if (_lineRenderer.positionCount == 0) return true;

        return Vector2.Distance(_lineRenderer.GetPosition(_lineRenderer.positionCount - 1), pos) > DrawManager.RESOLUTION;
    }

    public void AtDestinationLightUp()
    {
        // Get the LineRenderer component from the GameObject
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        // Get the current color for the LineRenderer
        Color currentColor = _lineColor;

        // Convert the color to HSB
        Color.RGBToHSV(currentColor, out float h, out float s, out float b);

        // Increase the brightness (b)
        b += _lightUpAtDestinationBy;

        // Clamp the brightness to the range [0, 1]
        b = Mathf.Clamp01(b);

        // Convert the HSB color back to RGB
        Color newColor = Color.HSVToRGB(h, s, b);

        // Create a Gradient with the modified color
        Gradient gradient = new();
        gradient.SetKeys(new GradientColorKey[] { new(newColor, 0f) }, new GradientAlphaKey[] { new(1f, 0f) });

        // Assign the gradient to the LineRenderer
        lineRenderer.colorGradient = gradient;
    }

    public void UpdateScore()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            return;
        }

        if (connectionToClient != null && connectionToClient.ToString() == "connection(0)")
        {
            networkManagerNoodlePair.GamePlayers[1].IncrementPlayerScore();
        }
        else
        {
            networkManagerNoodlePair.GamePlayers[0].IncrementPlayerScore();
        }
    }

    public void WaitYourTurnActive(string reasonText)
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            return;
        }

        // Find all objects of a specific type (e.g., GameObjects with a specific script attached)
        DrawManager[] drawManagers = FindObjectsOfType<DrawManager>();

        // Iterate through the found objects
        foreach (DrawManager drawManager in drawManagers)
        {
            drawManager.WaitYourTurnActiveInterface(reasonText);
        }
    }

    public void SetItemsMatched(Transform item1, Transform item2)
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            FindObjectOfType<DrawManager>().SetItemsMatched(item1, item2);
        }
        else
        {
            DrawManager[] drawManagers = FindObjectsOfType<DrawManager>();

            // Loop through the found DrawManagers
            foreach (DrawManager drawManager in drawManagers)
            {
                drawManager.SetItemsMatchedClientInterface(item1, item2);
            }
        }
    }
}

