using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [SerializeField] float completeDelayTime = 2f;

    [SerializeField] AudioClip finishClip;

    [SerializeField] AudioSource audioSource;

    bool levelCompleted = false;

    bool gameStarted = false;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
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
            levelCompleted = true;

            audioSource.PlayOneShot(finishClip);

            EventSystemNew.RaiseEvent(Event_Type.CHARACTER_FINISHED);

            Invoke("CompleteLevel", completeDelayTime);
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
}
