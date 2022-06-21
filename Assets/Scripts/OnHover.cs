using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHover : MonoBehaviour
{
    [SerializeField] GameObject hoverText;

    bool canShow = true;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    public void OnHoverEnter()
    {
        if (canShow)
        {
            hoverText.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        hoverText.SetActive(false);
    }

    private void GameStarted()
    {
        canShow = false;

        hoverText.SetActive(false);
    }
}
