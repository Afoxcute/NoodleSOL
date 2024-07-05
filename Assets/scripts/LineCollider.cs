using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LineCollider : NetworkBehaviour
{
    [SerializeField] private Transform _circleColliderTransform;

    [SerializeField] private AudioClip _matchSuccessClip;

    private bool _startDetecting;

    private void Awake()
    {
        StartCoroutine(DelayDetectionRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_startDetecting)
        {
            return;
        }

        Line parentLine = transform.parent.GetComponent<Line>();

        if (other.CompareTag("Line") && other.transform != _circleColliderTransform)//If other is Line and that line is not mine, destroy other
        {
            if (PlayerPrefs.GetString("GameMode") == "TwoPlayers")
            {
                if (isOwned) //intersected from same client
                {
                    parentLine.WaitYourTurnActive("You intersected another line");
                }
                // else if (other != null && other.GetComponentInSiblings<LineCollider>() != null) //intersected from other client
                // {
                //     Debug.Log("Called one");
                //     other.GetComponentInSiblings<LineCollider>().WaitYourTurnActive();
                // }
            }

            MainSceneManager.Instance.TriggerTryAgainSection("You intersected another line");
            Destroy(other.transform.parent.gameObject, .1f);
        }

        if (other.CompareTag("ItemToMatch") && other.transform.parent != MainSceneManager.Instance.CurrentItemBeingMatched.parent)//If other is ItemToMatch and is not 
                                                                                                                                  //part of item currently being matched, destroy me
        {
            if (PlayerPrefs.GetString("GameMode") == "TwoPlayers" && isOwned)
            {
                parentLine.WaitYourTurnActive("You matched the wrong item");
            }
            MainSceneManager.Instance.TriggerTryAgainSection("You matched the wrong item");
            Destroy(transform.parent.gameObject, .1f);
        }

        if (other.CompareTag("ItemToMatch") && other.transform.parent == MainSceneManager.Instance.CurrentItemBeingMatched.parent
        && other.transform != MainSceneManager.Instance.CurrentItemBeingMatched)//If other is ItemToMatch and is of the same parent and is not
                                                                                //current Item being matched
        {
            parentLine.LineAtDestination = true;//Line is at destination
            LightUpAndNotifyWhenMatched(parentLine, other);

            FindObjectOfType<LevelUI>().NumberOfItemsMatched++;
            parentLine.UpdateScore();

            AudioSource.PlayClipAtPoint(_matchSuccessClip, Camera.main.transform.position);
        }
    }

    public void WaitYourTurnActive()//for if intersected from other client
    {
        Debug.Log("Called two");
        Line parentLine = transform.parent.GetComponent<Line>();

        parentLine.WaitYourTurnActive("You intersected another line");
    }

    private void LightUpAndNotifyWhenMatched(Line parentLine, Collider2D other)
    {
        parentLine.AtDestinationLightUp();
        Color spriteCol = other.transform.GetComponent<SpriteRenderer>().color;
        spriteCol.a = 0.6f;
        other.transform.GetComponent<SpriteRenderer>().color = spriteCol;
        MainSceneManager.Instance.CurrentItemBeingMatched.GetComponent<SpriteRenderer>().color = spriteCol;
        parentLine.SetItemsMatched(MainSceneManager.Instance.CurrentItemBeingMatched, other.transform);
    }

    IEnumerator DelayDetectionRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        _startDetecting = true;
    }
}