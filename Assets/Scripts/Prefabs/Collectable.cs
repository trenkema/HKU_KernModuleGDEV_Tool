using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public bool pickupable { private set; get; }

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
        EventSystemNew.Subscribe(Event_Type.CHARACTER_FINISHED, CharacterFinished);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
        EventSystemNew.Unsubscribe(Event_Type.CHARACTER_FINISHED, CharacterFinished);
    }

    private void OnDestroy()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.COLLECTABLE_ADDED, -1);
    }

    private void Awake()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.COLLECTABLE_ADDED, 1);
    }

    private void GameStarted()
    {
        pickupable = true;
    }

    private void CharacterFinished()
    {
        pickupable = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && pickupable)
        {
            EventSystemNew.RaiseEvent(Event_Type.COLLECTABLE_COLLECTED);

            Destroy(gameObject);
        }
    }
}
