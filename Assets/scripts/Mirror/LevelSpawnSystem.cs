//using DapperDino.Tutorials.Lobby;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LevelRecentlyPlayedData
{
    public List<string> lettersLevelRecentlyPlayedList = new();
    public List<string> numbersLevelRecentlyPlayedList = new();
    public List<string> animalsLevelRecentlyPlayedList = new();
    public List<string> colorsLevelRecentlyPlayedList = new();
    public List<string> fruitsLevelRecentlyPlayedList = new();
    public List<string> birdsLevelRecentlyPlayedList = new();
    public List<string> insectsLevelRecentlyPlayedList = new();
    public List<string> vehiclesLevelRecentlyPlayedList = new();
    public List<string> environmentLevelRecentlyPlayedList = new();
}

public class LevelSpawnSystem : NetworkBehaviour
{
    private LevelRecentlyPlayedData _levelRecentlyPlayedData;

    [SerializeField] private List<Transform> _levels = new();

    public List<Transform> Levels => _levels;

    [SyncVar] private GameObject _syncedLevelToInstantiate;
    GameObject _levelToInstantiatePrefab;

    private static List<Transform> spawnPoints = new();

    private int nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    public override void OnStartServer() => NetworkManagerNoodlePair.OnServerReadied += SpawnLevel;

    [ServerCallback]
    private void OnDestroy() => NetworkManagerNoodlePair.OnServerReadied -= SpawnLevel;

    [Server]
    private void SpawnLevel(NetworkConnection conn)
    {
        if (conn.ToString() == "connection(0)")
        {
            SelectLevelToInstantiate();
            GameObject levelInstance = Instantiate(_levelToInstantiatePrefab, spawnPoints[nextIndex].position,
               spawnPoints[nextIndex].rotation);
            NetworkServer.Spawn(levelInstance);

            NetworkManagerNoodlePair.OnServerReadied -= SpawnLevel;

            RpcSyncLevelToInstantiate(levelInstance);
        }

        nextIndex++;
    }

    [Server]
    private void SelectLevelToInstantiate()
    {
        // Load the JSON string from PlayerPrefs
        string jsonString = PlayerPrefs.GetString("LevelRecentlyPlayedData");

        // Deserialize the JSON string back into the LevelRecentlyPlayedData object
        _levelRecentlyPlayedData = JsonUtility.FromJson<LevelRecentlyPlayedData>(jsonString);

        // If it's the first time, create a new PlayerData object
        _levelRecentlyPlayedData ??= new LevelRecentlyPlayedData();//if _levelRecentlyPlayedData is null

        List<string> levelRecentlyPlayedList = new();

        switch (transform.name)
        {
            case "Letters":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.lettersLevelRecentlyPlayedList;
                break;
            case "Numbers":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.numbersLevelRecentlyPlayedList;
                break;
            case "Animals":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.animalsLevelRecentlyPlayedList;
                break;
            case "Colors":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.colorsLevelRecentlyPlayedList;
                break;
            case "Fruits":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.fruitsLevelRecentlyPlayedList;
                break;
            case "Birds":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.birdsLevelRecentlyPlayedList;
                break;
            case "Insects":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.insectsLevelRecentlyPlayedList;
                break;
            case "Vehicles":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.vehiclesLevelRecentlyPlayedList;
                break;
            case "Environment":
                levelRecentlyPlayedList = _levelRecentlyPlayedData.environmentLevelRecentlyPlayedList;
                break;
        }

        // Create a list of eligible levels (not in the excluded names list)
        List<Transform> eligibleLevels = new();

        foreach (Transform level in _levels)
        {
            if (!levelRecentlyPlayedList.Contains(level.name))
            {
                eligibleLevels.Add(level);
            }
        }

        // Check if there are any eligible levels
        if (eligibleLevels.Count > 0)
        {
            // Randomly select a level from the eligible list
            Transform randomLevel = eligibleLevels[Random.Range(0, eligibleLevels.Count)];

            _levelToInstantiatePrefab = randomLevel.gameObject;
            PlayerPrefs.SetString("LastActiveLevel", randomLevel.name);
            PlayerPrefs.Save();
            levelRecentlyPlayedList.Add(randomLevel.name);
            SaveData();
        }
        else //All levels have been played
        {
            levelRecentlyPlayedList.Clear();
            SaveData();

            if (PlayerPrefs.GetString("LastActiveLevel") != null && PlayerPrefs.GetString("LastActiveLevel") != _levels[0].name)
            {
                _levelToInstantiatePrefab = _levels[0].gameObject;

                PlayerPrefs.SetString("LastActiveLevel", _levels[0].name);
                PlayerPrefs.Save();
                levelRecentlyPlayedList.Add(_levels[0].name);
                SaveData();
            }
            else
            {
                // Create a list of eligible level (not the first level)
                List<Transform> eligibleLevelAno = new(_levels);
                eligibleLevelAno.RemoveAt(0); // Remove the first level from the list

                // Check if there are any eligible level
                if (eligibleLevelAno.Count > 0)
                {
                    // Randomly select a level from the eligible list
                    Transform randomLevel = eligibleLevelAno[Random.Range(0, eligibleLevelAno.Count)];
                    _levelToInstantiatePrefab = randomLevel.gameObject;

                    // Store the name of the newly selected level in PlayerPrefs
                    PlayerPrefs.SetString("LastActiveLevel", randomLevel.name);
                    PlayerPrefs.Save();
                    levelRecentlyPlayedList.Add(randomLevel.name);
                    SaveData();
                }
            }

        }
    }

    [ClientRpc]
    private void RpcSyncLevelToInstantiate(GameObject levelToInstantiate)
    {
        _syncedLevelToInstantiate = levelToInstantiate;
    }

    private void SaveData()
    {
        // Serialize the LevelRecentlyPlayedData object to a JSON string
        string jsonString = JsonUtility.ToJson(_levelRecentlyPlayedData);

        // Save the JSON string to PlayerPrefs
        PlayerPrefs.SetString("LevelRecentlyPlayedData", jsonString);

        // Save the PlayerPrefs to disk
        PlayerPrefs.Save();
    }
}
