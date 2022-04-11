using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWaypoint : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    [SerializeField] float speed = 2f;

    [SerializeField] bool flipOnEnd = false;

    int currentWaypointIndex = 0;

    private void Update()
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
