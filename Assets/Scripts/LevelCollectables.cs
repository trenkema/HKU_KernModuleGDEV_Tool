using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelCollectables : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI collectedText;

    private int pickedupCollectables = 0;
    private int collectablesAdded = 0;

    private int totalCollectables = 0;

    private void OnEnable()
    {
        EventSystemNew<int>.Subscribe(Event_Type.COLLECTABLE_ADDED, CollectableAdded);
        EventSystemNew.Subscribe(Event_Type.COLLECTABLE_COLLECTED, CollectablePickedup);
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew<int>.Unsubscribe(Event_Type.COLLECTABLE_ADDED, CollectableAdded);
        EventSystemNew.Unsubscribe(Event_Type.COLLECTABLE_COLLECTED, CollectablePickedup);
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void CollectableAdded(int _addedOrRemoved)
    {
        collectablesAdded += _addedOrRemoved;

        collectedText.text = pickedupCollectables + " / " + totalCollectables;
    }

    private void CollectablePickedup()
    {
        pickedupCollectables++;
    }

    private void GameStarted()
    {
        totalCollectables = collectablesAdded;
        collectedText.text = pickedupCollectables + " / " + totalCollectables;
    }
}
