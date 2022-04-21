using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class LevelEntryData : MonoBehaviour
{
    public int id;

    public string levelName;

    public string authorName;

    public string textFileURL;

    public Image levelIcon;

    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] TextMeshProUGUI authorNameText;

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        nameText.text = levelName;

        authorNameText.text = "Created By: " + authorName;

        levelIcon.sprite = levelIcon.sprite;
    }

    public void LoadLevel()
    {
        EventSystemNew<string>.RaiseEvent(Event_Type.LOAD_LEVEL, textFileURL);
    }
}
