using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPoint : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] float destroyTime = 5f;

    [SerializeField] bool destroy = false;

    float elapsedTime = 0f;

    bool startTimer = false;

    GameObject spawnedPlayer;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);

        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);

        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void OnDestroy()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.START_ADDED, -1);

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }
    }

    private void Start()
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.START_ADDED, 1);
    }

    private void Update()
    {
        if (destroy && startTimer)
            DestroyStartPoint();
    }

    private void LevelCompleted()
    {
        startTimer = false;

        elapsedTime = destroyTime;
    }

    private void GameStarted()
    {
        startTimer = true;

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }

        spawnedPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);

        Debug.Log("Spawned Player: " + spawnedPlayer.name);

        EventSystemNew<Transform>.RaiseEvent(Event_Type.PLAYER_TRANSFORM, spawnedPlayer.transform);

        elapsedTime = destroyTime;

        Debug.Log("Game Started");
    }

    private void DestroyStartPoint()
    {
        elapsedTime -= Time.deltaTime;

        float alpha = elapsedTime / destroyTime;

        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);

        if (elapsedTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
