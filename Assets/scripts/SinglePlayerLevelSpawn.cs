using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerLevelSpawn : MonoBehaviour
{
    [SerializeField] private List<GameObject> levelSpawnSystems = new();
    [SerializeField] private GameObject drawManager;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            return;
        }

        switch (PlayerPrefs.GetString("LevelName"))
        {
            case "Letters":
                Instantiate(levelSpawnSystems[0]);
                break;
            case "Numbers":
                Instantiate(levelSpawnSystems[1]);
                break;
            case "Animals":
                Instantiate(levelSpawnSystems[2]);
                break;
            case "Colors":
                Instantiate(levelSpawnSystems[3]);
                break;
            case "Fruits":
                Instantiate(levelSpawnSystems[4]);
                break;
            case "Birds":
                Instantiate(levelSpawnSystems[5]);
                break;
            case "Insects":
                Instantiate(levelSpawnSystems[6]);
                break;
            case "Vehicles":
                Instantiate(levelSpawnSystems[7]);
                break;
            case "Environment":
                Instantiate(levelSpawnSystems[8]);
                break;
        }

        Instantiate(drawManager);
    }
}
