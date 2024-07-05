using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelRecentlyPlayedDataSinglePlayer
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

public class RandomLevelNotRecentlyPickedSelector : MonoBehaviour
{
    [SerializeField] private Button _anotherSetButtonPauseMenu, _anotherSetButtonGoodJob, _anotherSetButtonTryAgain;

    private List<Transform> _levels = new();

    private LevelRecentlyPlayedDataSinglePlayer _levelRecentlyPlayedData;

    private void OnEnable()
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            return;
        }

        _levels = GetComponent<LevelSpawnSystem>().Levels;

        // Find all objects of type AnotherSetButtonType
        AnotherSetButtonType[] buttons = FindObjectsOfType<AnotherSetButtonType>(true);

        // Loop through the found buttons
        foreach (AnotherSetButtonType button in buttons)
        {
            // Check the name of the parent's parent GameObject
            GameObject parentParent = button.gameObject.transform.parent?.parent?.gameObject;

            // Get the Button component on the current button
            Button buttonComponent = button.GetComponent<Button>();

            if (buttonComponent == null)
            {
                Debug.LogError("Button component not found on: " + button.gameObject.name);
                continue;
            }

            switch (parentParent?.name)
            {
                case "PauseMenu":
                    // Assign the button component to _anotherSetButtonPauseMenu
                    _anotherSetButtonPauseMenu = buttonComponent;
                    break;
                case "GoodJobSection":
                    // Assign the button component to _anotherSetButtonGoodJob
                    _anotherSetButtonGoodJob = buttonComponent;
                    break;
                case "TryAgainSection":
                    // Assign the button component to _anotherSetButtonTryAgain
                    _anotherSetButtonTryAgain = buttonComponent;
                    break;
            }
        }


        if (transform.name == "Colors")
        {
            _anotherSetButtonPauseMenu.gameObject.SetActive(false);
            _anotherSetButtonGoodJob.gameObject.SetActive(false);
            _anotherSetButtonTryAgain.gameObject.SetActive(false);

            Vector2 spacing = _anotherSetButtonGoodJob.GetComponentInParent<GridLayoutGroup>(true).spacing;
            spacing.x = 136;
            _anotherSetButtonGoodJob.GetComponentInParent<GridLayoutGroup>(true).spacing = spacing;
            _anotherSetButtonTryAgain.GetComponentInParent<GridLayoutGroup>(true).spacing = spacing;
        }
        else
        {
            _anotherSetButtonPauseMenu.gameObject.SetActive(true);
            _anotherSetButtonGoodJob.gameObject.SetActive(true);
            _anotherSetButtonTryAgain.gameObject.SetActive(true);
            _anotherSetButtonPauseMenu.onClick.AddListener(HandleLevelSelection);
            _anotherSetButtonGoodJob.onClick.AddListener(HandleLevelSelection);
            _anotherSetButtonTryAgain.onClick.AddListener(HandleLevelSelection);

            Vector2 spacing = _anotherSetButtonGoodJob.GetComponentInParent<GridLayoutGroup>(true).spacing;
            spacing.x = 10;
            _anotherSetButtonGoodJob.GetComponentInParent<GridLayoutGroup>(true).spacing = spacing;
            _anotherSetButtonTryAgain.GetComponentInParent<GridLayoutGroup>(true).spacing = spacing;
        }

        HandleLevelSelection();
    }

    private void OnDisable()
    {
        if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
        {
            return;
        }

        if (transform.name == "Colors")
        {
            return;
        }

        _anotherSetButtonPauseMenu.onClick.RemoveListener(HandleLevelSelection);
        _anotherSetButtonGoodJob.onClick.RemoveListener(HandleLevelSelection);
        _anotherSetButtonTryAgain.onClick.RemoveListener(HandleLevelSelection);
    }

    private void HandleLevelSelection()
    {
        // Load the JSON string from PlayerPrefs
        string jsonString = PlayerPrefs.GetString("LevelRecentlyPlayedDataSinglePlayer");

        // Deserialize the JSON string back into the LevelRecentlyPlayedDataSinglePlayer object
        _levelRecentlyPlayedData = JsonUtility.FromJson<LevelRecentlyPlayedDataSinglePlayer>(jsonString);

        // If it's the first time, create a new PlayerData object
        _levelRecentlyPlayedData ??= new LevelRecentlyPlayedDataSinglePlayer();//if _levelRecentlyPlayedData is null

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

            Instantiate(randomLevel.gameObject);
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
                Instantiate(_levels[0].gameObject);

                PlayerPrefs.SetString("LastActiveLevel", _levels[0].name);
                PlayerPrefs.Save();
                levelRecentlyPlayedList.Add(_levels[0].name);
                SaveData();
            }
            else
            {
                // Create a list of eligible levels (not the first level)
                List<Transform> eligibleLevelsAno = new(_levels);
                eligibleLevelsAno.RemoveAt(0); // Remove the first level from the list

                // Check if there are any eligible levels
                if (eligibleLevelsAno.Count > 0)
                {
                    // Randomly select a level from the eligible list
                    Transform randomLevel = eligibleLevelsAno[Random.Range(0, eligibleLevelsAno.Count)];
                    Instantiate(randomLevel.gameObject);

                    // Store the name of the newly selected level in PlayerPrefs
                    PlayerPrefs.SetString("LastActiveLevel", randomLevel.name);
                    PlayerPrefs.Save();
                    levelRecentlyPlayedList.Add(randomLevel.name);
                    SaveData();
                }
            }

        }
    }

    private void SaveData()
    {
        // Serialize the LevelRecentlyPlayedDataSinglePlayer object to a JSON string
        string jsonString = JsonUtility.ToJson(_levelRecentlyPlayedData);

        // Save the JSON string to PlayerPrefs
        PlayerPrefs.SetString("LevelRecentlyPlayedDataSinglePlayer", jsonString);

        // Save the PlayerPrefs to disk
        PlayerPrefs.Save();
    }
}

