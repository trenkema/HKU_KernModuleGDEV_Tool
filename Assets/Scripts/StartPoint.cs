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

    bool gameStarted = false;

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
        if (destroy && gameStarted)
            DestroyStartPoint();
    }

    private void LevelCompleted()
    {
        gameStarted = false;

        elapsedTime = destroyTime;
    }

    private void GameStarted()
    {
        gameStarted = true;

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }

        spawnedPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);

        elapsedTime = destroyTime;
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
