using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [Header("Set individually")]
    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private Transform _itemsToMatchParent;

    public Transform ItemsToMatchPaent => _itemsToMatchParent;

    [Header("Set in prefab")]
    [SerializeField] private AudioClip _buttonClick;

    [SerializeField] private TextMeshProUGUI _totalNumberOfItemsToMatchText, _numberOfItemsMatchedText;

    public int NumberOfItemsMatched { get; set; }

    // Update is called once per frame
    void Update()
    {
        _totalNumberOfItemsToMatchText.text = "/" + _itemsToMatchParent.childCount;
        _numberOfItemsMatchedText.color = MainSceneManager.Instance.LineColor;
        _numberOfItemsMatchedText.text = NumberOfItemsMatched.ToString();

        if (NumberOfItemsMatched == _itemsToMatchParent.childCount)
        {
            MainSceneManager.Instance.TriggerGoodJobSection();
        }
    }

    public void RevertAlphasOfAllItemsToMatch()
    {
        FindSpriteRenderersRecursively(_itemsToMatchParent);
    }

    public void SetPauseMenuActive()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePLayer")
        {
            FindObjectOfType<PauseMenu>(true).gameObject.SetActive(true);
        }
        else
        {
            FindObjectOfType<PauseMenuMultiplayer>(true).gameObject.SetActive(true);
        }
    }

    public void PlayButtonClick()
    {
        AudioSource.PlayClipAtPoint(_buttonClick, Camera.main.transform.position);
    }

    void FindSpriteRenderersRecursively(Transform parent)
    {
        // Loop through each child of the parent transform
        foreach (Transform child in parent)
        {
            // Check if the child has a SpriteRenderer component

            // If a SpriteRenderer component is found, you can do something with it here
            if (child.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                Color color = spriteRenderer.color;
                color.a = 0.15f;
                spriteRenderer.color = color;
            }

            // Recursively search in the child's children
            FindSpriteRenderersRecursively(child);
        }
    }
}