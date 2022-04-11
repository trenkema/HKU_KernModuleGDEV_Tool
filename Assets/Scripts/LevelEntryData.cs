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

    public string textFileURL;

    public Image levelIcon;

    [SerializeField] TextMeshProUGUI nameText;

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        nameText.text = levelName;
        levelIcon.sprite = levelIcon.sprite;
    }

    public void LoadLevel()
    {
        StartCoroutine(DownloadLevelTextFile(textFileURL));
    }

    private IEnumerator DownloadLevelTextFile(string textFileURL)
    {
        UnityWebRequest www = UnityWebRequest.Get(textFileURL);

        yield return www.SendWebRequest();

        string filePath = Application.dataPath + "/" + "LevelData.json";

        File.WriteAllText(filePath, www.downloadHandler.text);

        AssetDatabase.Refresh();

        yield return new WaitForSeconds(1f);

        EventSystemNew.RaiseEvent(Event_Type.LOAD_LEVEL);

        yield return new WaitForSeconds(1f);

        EventSystemNew.RaiseEvent(Event_Type.CLOSE_DOWNLOAD_MENU);
    }
}
