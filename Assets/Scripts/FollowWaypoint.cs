using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowWaypoint : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    [SerializeField] Image[] waypointImages;

    [SerializeField] float speed = 2f;

    [SerializeField] bool flipOnEnd = false;

    int currentWaypointIndex = 0;

    bool gameStarted = false;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void Update()
    {
        if (gameStarted)
        {
            if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < .1f)
            {
                if (flipOnEnd)
                {
                    transform.Rotate(0, 180, 0);
                }

                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }
            }

            transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, Time.deltaTime * speed);
        }
    }

    private void GameStarted()
    {
        gameStarted = true;

        foreach (var image in waypointImages)
        {
            image.enabled = false;
        }
    }
}
