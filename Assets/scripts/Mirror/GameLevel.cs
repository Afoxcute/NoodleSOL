using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameLevel : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerOneName, _playerTwoName, _playerOneScore, _playerTwoScore;

    private LevelUI _levelUI;

    private const string PlayerPrefsNameKey = "PlayerName";

    private NetworkGamePlayerNoodlePair _playerOne, _playerTwo;

    private NetworkManagerNoodlePair networkManagerNoodlePair;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            _playerOneName.gameObject.SetActive(false);
            _playerTwoName.gameObject.SetActive(false);
            _playerOneScore.gameObject.SetActive(false);
            _playerTwoScore.gameObject.SetActive(false);

            return;
        }

        _levelUI = FindObjectOfType<LevelUI>();

        networkManagerNoodlePair = NetworkManager.singleton as NetworkManagerNoodlePair;

        _playerOne = networkManagerNoodlePair.GamePlayers[0];
        _playerTwo = networkManagerNoodlePair.GamePlayers[1];

        _playerOneName.text = _playerOne.PlayerName;
        _playerTwoName.text = _playerTwo.PlayerName;
    }

    void Update()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            return;
        }

        _playerOneScore.text = _playerOne.PlayerScore.ToString();
        _playerTwoScore.text = _playerTwo.PlayerScore.ToString();

        //If equal to eachother and what is left is less than two
        if ((_playerOne.PlayerScore == _playerTwo.PlayerScore) && (_levelUI.ItemsToMatchPaent.childCount - _levelUI.NumberOfItemsMatched) < 2)
        {
            MainSceneManager.Instance.TriggerTieSection();
        }
        else if (_levelUI.NumberOfItemsMatched == _levelUI.ItemsToMatchPaent.childCount)
        {
            string winnerNameText = "", winnerScoreText = "", otherPlayerNameText = "", otherPlayerScoreText = "";

            if (_playerOne.PlayerScore > _playerTwo.PlayerScore)
            {
                winnerNameText = _playerOne.PlayerName;
                winnerScoreText = _playerOne.PlayerScore.ToString();
                otherPlayerNameText = _playerTwo.PlayerName;
                otherPlayerScoreText = _playerTwo.PlayerScore.ToString();
            }
            else if (_playerOne.PlayerScore < _playerTwo.PlayerScore)
            {
                winnerNameText = _playerTwo.PlayerName;
                winnerScoreText = _playerTwo.PlayerScore.ToString();
                otherPlayerNameText = _playerOne.PlayerName;
                otherPlayerScoreText = _playerOne.PlayerScore.ToString();
            }

            MainSceneManager.Instance.TriggerMultiplayerGameDoneSection(winnerNameText, winnerScoreText, otherPlayerNameText, otherPlayerScoreText);
        }
    }
}
