using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Specialized;
using System.Linq;

public class HighScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI highScoresText;

    [SerializeField] int maxScores = 3;

    List<float> scores = new List<float>();
    List<string> names = new List<string>();

    int highscoreDatabaseID = 8667;

    string playerName = "";

    private void OnEnable()
    {
        EventSystemNew<float, string>.Subscribe(Event_Type.UPLOAD_SCORE, UploadTime);
    }

    private void OnDisable()
    {
        EventSystemNew<float, string>.Unsubscribe(Event_Type.UPLOAD_SCORE, UploadTime);
    }

    private void Awake()
    {
        LootLockerSDKManager.GetPlayerName((response) =>
        {
            if (response.success)
            {
                playerName = response.name;
            }
            else
            {
                Debug.Log("Error getting player name");
            }
        });
    }

    private void UploadTime(float _time, string _assetID)
    {
        LootLockerSDKManager.GetMemberRank(highscoreDatabaseID.ToString(), int.Parse(_assetID), (levelResponse) =>
        {
            if (levelResponse.statusCode != 200)
            {
                return;
            }

            scores = new List<float>();
            names = new List<string>();

            if (levelResponse.member_id == string.Empty)
            {
                Debug.Log("Level Response Doesn't exist");

                string data = _time + ":" + playerName;
                LootLockerSDKManager.SubmitScore(_assetID, 0, highscoreDatabaseID.ToString(), data, (scoreResponse) =>
                {
                    if (scoreResponse.statusCode == 200)
                    {
                        Debug.Log("First Score");

                        scores.Add(_time);
                        names.Add(playerName);
                        DisplayScores();
                    }
                    else
                    {
                        Debug.Log("Failed to upload score: " + scoreResponse.Error);
                    }
                });

                return;
            }

            Debug.Log("Ah Oh");

            if (levelResponse.metadata.Contains("/"))
            {
                string[] splitMetadata = levelResponse.metadata.Split(char.Parse("/"));

                for (int i = 0; i < splitMetadata.Length; i++)
                {
                    string[] scoreAndName = splitMetadata[i].Split(char.Parse(":"));

                    bool result;
                    float number;
                    result = float.TryParse(scoreAndName[0], out number);

                    if (result)
                    {
                        scores.Add(number);
                        names.Add(scoreAndName[1]);
                    }
                }
            }
            else
            {
                string[] scoreAndName = levelResponse.metadata.Split(char.Parse(":"));

                bool result;
                float number;
                result = float.TryParse(scoreAndName[0], out number);

                if (result)
                {
                    scores.Add(number);
                    names.Add(scoreAndName[1]);
                }
            }
            
            Debug.Log("Score List Count: " + scores.Count);

            for (int i = 0; i < scores.Count; i++)
            {
                if (_time < scores[i])
                {
                    if (scores.Count == maxScores)
                    {
                        scores.RemoveAt(scores.Count - 1);
                        names.RemoveAt(names.Count - 1);
                    }

                    scores.Insert(i, _time);
                    names.Insert(i, playerName);

                    break;
                }
                else if (i == scores.Count - 1 && scores.Count != maxScores)
                {
                    scores.Add(_time);
                    names.Add(playerName);
                }
            }

            string metadata = "";

            for (int i = 0; i < scores.Count; i++)
            {
                if (i != scores.Count - 1)
                {
                    metadata += (scores[i] + ":" + names[i] + "/");
                }
                else
                {
                    metadata += scores[i] + ":" + names[i];
                }
            }

            LootLockerSDKManager.SubmitScore(_assetID, levelResponse.score + 1, highscoreDatabaseID.ToString(), metadata, (scoreResponse) =>
            {

            });

            DisplayScores();
        });
    }

    private void DisplayScores()
    {
        highScoresText.text = "";

        for (int i = 0; i < scores.Count; i++)
        {
            highScoresText.text += ((i + 1) + ": " + TimeSpan.FromSeconds(scores[i]).ToString("mm':'ss'.'ff") + " - " + names[i] + "<br>");
        }
    }
}
