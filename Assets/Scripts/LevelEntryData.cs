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

    public string assetID;

    public string levelName;

    public string textFileURL;

    public Image levelIcon;

    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] GameObject deleteButton;

    [SerializeField] GameObject activateButton;

    private void Awake()
    {
        deleteButton.SetActive(false);
        activateButton.SetActive(false);
    }

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        nameText.text = levelName;

        levelIcon.sprite = levelIcon.sprite;
    }

    public void LoadLevel()
    {
        EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, true);

        EventSystemNew<string, LevelEntryData>.RaiseEvent(Event_Type.LOAD_LEVEL, textFileURL, this);
    }

    public void DeactiveLevel()
    {
        EventSystemNew<string>.RaiseEvent(Event_Type.DEACTIVATE_LEVEL, assetID);
    }

    public void ActivateLevel()
    {
        EventSystemNew<string>.RaiseEvent(Event_Type.ACTIVATE_LEVEL, assetID);
    }

    public void EnableDeleteButton()
    {
        deleteButton.SetActive(true);
    }

    public void EnableActivateButton()
    {
        activateButton.SetActive(true);
    }
}
