using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] GameObject[] hoverItems;

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
            foreach (var item in hoverItems)
            {
                item.SetActive(true);
            }
        }
    }

    public void OnHoverExit()
    {
        if (canShow)
        {
            foreach (var item in hoverItems)
            {
                item.SetActive(false);
            }
        }
    }

    private void GameStarted()
    {
        canShow = false;

        foreach (var item in hoverItems)
        {
            item.SetActive(false);
        }
    }
}
