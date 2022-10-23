using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform playerTransform;

    [SerializeField] Vector3 offset;

    [SerializeField] float smoothFactor = 5f;

    private void OnEnable()
    {
        EventSystemNew<Transform>.Subscribe(Event_Type.PLAYER_TRANSFORM, SetPlayerTransform);
    }

    private void OnDisable()
    {
        EventSystemNew<Transform>.Unsubscribe(Event_Type.PLAYER_TRANSFORM, SetPlayerTransform);
    }

    private void FixedUpdate()
    {
        Follow();    
    }

    private void Follow()
    {
        if (playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + offset;

            Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, smoothFactor * Time.fixedDeltaTime);

            transform.position = smoothPosition;
        }
    }

    private void SetPlayerTransform(Transform _playerTransform)
    {
        playerTransform = _playerTransform;
    }
}
