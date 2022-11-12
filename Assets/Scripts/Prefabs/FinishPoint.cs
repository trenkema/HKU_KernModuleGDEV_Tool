using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] GameObject missingCollectablesText;

    [SerializeField] float completeDelayTime = 2f;

    [SerializeField] Animator animator;

    [SerializeField] AudioClip finishClip;

    [SerializeField] AudioSource audioSource;

    bool levelCompleted = false;

    bool gameStarted = false;

    bool canFinish = false;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
        EventSystemNew.Subscribe(Event_Type.ALL_COLLECTABLES_COLLECTED, AllCollectablesCollected);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
        EventSystemNew.Unsubscribe(Event_Type.ALL_COLLECTABLES_COLLECTED, AllCollectablesCollected);
    }

    private void OnDestroy()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.FINISH_ADDED, -1);
    }

    private void Start()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.FINISH_ADDED, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !levelCompleted && gameStarted)
        {
            if (canFinish)
            {
                levelCompleted = true;

                animator.SetBool("Finished", true);

                audioSource.PlayOneShot(finishClip);

                EventSystemNew.RaiseEvent(Event_Type.CHARACTER_FINISHED);

                Invoke("CompleteLevel", completeDelayTime);
            }
            else
            {
                missingCollectablesText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !levelCompleted && gameStarted)
        {
            missingCollectablesText.SetActive(false);
        }
    }

    private void GameStarted()
    {
        gameStarted = true;
    }

    private void CompleteLevel()
    {
        EventSystemNew.RaiseEvent(Event_Type.LEVEL_COMPLETED);
    }

    private void AllCollectablesCollected()
    {
        canFinish = true;
    }
}
