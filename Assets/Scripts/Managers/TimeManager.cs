using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using LootLocker.Requests;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private bool timerRunning = false;
    private float timeElapsed = 0f;
    private string levelID = "";

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, StartTimer);
        EventSystemNew.Subscribe(Event_Type.CHARACTER_FINISHED, TimerFinished);
        EventSystemNew.Subscribe(Event_Type.CHARACTER_DIED, StopTimer);
        EventSystemNew<string>.Subscribe(Event_Type.SET_LEVEL_ID, SetLevelID);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, StartTimer);
        EventSystemNew.Unsubscribe(Event_Type.CHARACTER_FINISHED, TimerFinished);
        EventSystemNew.Unsubscribe(Event_Type.CHARACTER_DIED, StopTimer);
        EventSystemNew<string>.Unsubscribe(Event_Type.SET_LEVEL_ID, SetLevelID);
    }

    private void Awake()
    {
        timeText.text = "00:00.00";
    }

    private void Update()
    {
        if (timerRunning)
        {
            timeElapsed += Time.deltaTime;

            DisplayTime();
        }
    }

    private void StartTimer()
    {
        HUDManager.Instance.timerUI.SetActive(true);

        timeElapsed = 0f;
        timerRunning = true;
    }

    private void TimerFinished()
    {
        timerRunning = false;

        Debug.Log("LevelID: " + levelID);

        EventSystemNew<float, string, bool>.RaiseEvent(Event_Type.UPLOAD_SCORE, timeElapsed, levelID, levelID != string.Empty);
    }

    private void StopTimer()
    {
        timerRunning = false;
    }

    private void DisplayTime()
    {
        timeText.text = TimeSpan.FromSeconds(timeElapsed).ToString("mm':'ss'.'ff");
    }

    private void SetLevelID(string _levelID)
    {
        levelID = _levelID;
    }
}
