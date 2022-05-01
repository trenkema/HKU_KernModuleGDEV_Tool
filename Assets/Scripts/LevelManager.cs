using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEditor;
using LootLocker.Admin;

public enum LevelManagerType
{
    TileManager,
    PrefabManager
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] TMP_InputField levelNameInputField;

    [SerializeField] GameObject missingStartOrFinishUI;

    [SerializeField] GameObject editorUI;
    [SerializeField] GameObject inGameUI;

    [SerializeField] GameObject loadingLevelUI;

    [SerializeField] GameObject levelUploadUI;

    [SerializeField] GameObject downloadLevelUI;

    [SerializeField] GameObject failedToLoadUI;

    [SerializeField] GameObject levelCompletedUI;

    [SerializeField] GameObject gameOverUI;

    [SerializeField] GameObject levelEntryDisplayItem;

    [SerializeField] Transform levelDataEntryContent;

    List<GameObject> displayItems = new List<GameObject>();

    string levelName;

    string playerName;

    int currentAssetID = -1;

    int startPointAdded = 0;
    int finishPointAdded = 0;

    bool retrieveOwnLevelsOnly = false;

    private void OnEnable()
    {
        EventSystemNew<bool>.Subscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);

        EventSystemNew<string>.Subscribe(Event_Type.LOAD_LEVEL, LoadLevelData);

        EventSystemNew<string, string>.Subscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);

        EventSystemNew<int>.Subscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Subscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void OnDisable()
    {
        EventSystemNew<bool>.Unsubscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);

        EventSystemNew<string>.Unsubscribe(Event_Type.LOAD_LEVEL, LoadLevelData);

        EventSystemNew<string, string>.Unsubscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);

        EventSystemNew<int>.Unsubscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Unsubscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void Start()
    {
        LootLockerSDKManager.GetPlayerName(response =>
        {
            if (!response.success)
            {
                failedToLoadUI.SetActive(true);
                return;
            }

            playerName = response.name;
        });
    }

    private void SaveLevel()
    {
        AllLevelData allLevelData = new AllLevelData();

        allLevelData.tileLevelData = TileLevelManager.Instance.SaveLevel();
        allLevelData.prefabLevelData = PrefabLevelEditor.Instance.SaveLevel();

        string json = JsonUtility.ToJson(allLevelData, true);

        File.WriteAllText(Application.dataPath + "/LevelData.json", json);

        Debug.Log("Level Saved");
    }

    private void SetLoadingScreen(bool _isActive)
    {
        downloadLevelUI.SetActive(false);
        loadingLevelUI.SetActive(_isActive);
    }

    private void LoadLevel()
    {
        string json = File.ReadAllText(Application.dataPath + "/LevelData.json");

        AllLevelData levelData = JsonUtility.FromJson<AllLevelData>(json);

        TileLevelManager.Instance.LoadLevel(levelData.tileLevelData);
        PrefabLevelEditor.Instance.LoadLevel(levelData.prefabLevelData);
    }

    private void LoadLevelData(string _textFileURL)
    {
        StartCoroutine(DownloadLevelTextFile(_textFileURL));
    }

    private IEnumerator DownloadLevelTextFile(string textFileURL)
    {
        UnityWebRequest www = UnityWebRequest.Get(textFileURL);

        yield return www.SendWebRequest();

        string filePath = Application.dataPath + "/" + "LevelData.json";

        File.WriteAllText(filePath, www.downloadHandler.text);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, true);

        yield return new WaitForSeconds(1f);

        EventSystemNew.RaiseEvent(Event_Type.LOAD_LEVEL);

        yield return new WaitForSeconds(0.5f);

        string json = File.ReadAllText(Application.dataPath + "/LevelData.json");

        AllLevelData levelData = JsonUtility.FromJson<AllLevelData>(json);

        TileLevelManager.Instance.LoadLevel(levelData.tileLevelData);
        PrefabLevelEditor.Instance.LoadLevel(levelData.prefabLevelData);

        EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, false);
    }

    public void CreateLevel()
    {
        levelName = levelNameInputField.text;

        LootLockerSDKManager.CreatingAnAssetCandidate(levelName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Created Asset");

                UploadLevelData(response.asset_candidate_id, 0);
            }
        });
    }

    public void TakeScreenshot()
    {
        string filePath = Application.dataPath + "/";

        Debug.Log("FilePath: " + filePath);

        ScreenCapture.CaptureScreenshot(Path.Combine(filePath, "Level-Screenshot.png"));
    }

    IEnumerator WaitScreenshot()
    {
        TakeScreenshot();

        yield return new WaitForSeconds(0.5f);

        levelUploadUI.SetActive(true);
    }

    public void OpenUploadLevelUI()
    {
        if (startPointAdded > 0 && finishPointAdded > 0)
        {
            SaveLevel();

            StartCoroutine(WaitScreenshot());
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);
        }
    }

    public void UploadLevelData(int _levelID, int _versionID)
    {
        string screenshotFilePath = Application.dataPath + "/" + "Level-Screenshot.png";

        LootLocker.LootLockerEnums.FilePurpose screenshotFileType = LootLocker.LootLockerEnums.FilePurpose.primary_thumbnail;

        LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, screenshotFilePath, "Level-Screenshot.png", screenshotFileType, (screenshotResponse) =>
        {
            if (screenshotResponse.success)
            {
                string textFilePath = Application.dataPath + "/" + "LevelData.json";

                LootLocker.LootLockerEnums.FilePurpose textFileType = LootLocker.LootLockerEnums.FilePurpose.file;

                LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, textFilePath, "LevelData.json", textFileType, (textResponse) =>
                {
                    if (textResponse.success)
                    {
                        Dictionary<string, string> KV = new Dictionary<string, string>();
                        KV.Add("playerName", playerName);
                        KV.Add("assetID", _levelID.ToString());
                        KV.Add("version", _versionID.ToString());

                        LootLockerSDKManager.UpdatingAnAssetCandidate(_levelID, true, (updatedResponse) =>
                        {
                            if (updatedResponse.success)
                            {
                                LootLockerSDKManager.GetSingleKeyPersistentStorage("playerID", (response) =>
                                {
                                    if (response.success)
                                    {
                                        if (response.payload == null)
                                        {
                                            LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
                                            {
                                                for (int i = 0; i < response.assets.Length; i++)
                                                {
                                                    if (levelName == response.assets[i].name)
                                                    {
                                                        for (int ii = 0; ii < response.assets[i].storage.Length; ii++)
                                                        {
                                                            if (response.assets[i].storage[ii].key == "assetID")
                                                            {
                                                                if (_levelID.ToString() == response.assets[i].storage[ii].value)
                                                                {
                                                                    LootLockerSDKManager.UpdateOrCreateKeyValue("playerID", (response.assets[i].asset_candidate.created_by_player_id).ToString(), (getPersistentStorageResponse) =>
                                                                    {

                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }, null, true);
                                        }
                                    }
                                });
                            }
                        }, null, KV);
                    }
                    else
                    {
                        Debug.Log("Error uploading asset candidate");
                    }
                });
            }
            else
            {
                Debug.Log("Error uploading asset candidate");
            }
        });
    }

    public void DownloadLevelData()
    {
        foreach (var displayItem in displayItems)
        {
            Destroy(displayItem);
        }

        displayItems.Clear();

        LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
        {
            if (retrieveOwnLevelsOnly)
            {
                LootLockerSDKManager.GetSingleKeyPersistentStorage("playerID", (playerIDResponse) =>
                {
                    if (playerIDResponse.success)
                    {
                        if (playerIDResponse.payload != null)
                        {
                            for (int i = 0; i < response.assets.Length; i++)
                            {
                                if (response.assets[i].asset_candidate.created_by_player_id.ToString() == playerIDResponse.payload.value)
                                {
                                    GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

                                    displayItems.Add(displayItem);

                                    displayItem.transform.SetParent(levelDataEntryContent);

                                    LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

                                    levelEntryData.id = i;
                                    levelEntryData.levelName = response.assets[i].name;
                                    levelEntryData.EnableDeleteButton();

                                    for (int ii = 0; ii < response.assets[i].storage.Length; ii++)
                                    {
                                        if (response.assets[i].storage[ii].key == "assetID")
                                        {
                                            levelEntryData.assetID = response.assets[i].storage[ii].value;
                                        }
                                    }

                                    LootLockerFile[] levelImageFiles = response.assets[i].files;

                                    StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));

                                    levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                LootLockerSDKManager.GetSingleKeyPersistentStorage("playerID", (playerIDResponse) =>
                {
                    string playerID = "";

                    if (playerIDResponse.success)
                    {
                        if (playerIDResponse.payload != null)
                        {
                            playerID = playerIDResponse.payload.value;
                        }
                    }

                    for (int i = 0; i < response.assets.Length; i++)
                    {
                        GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

                        displayItems.Add(displayItem);

                        displayItem.transform.SetParent(levelDataEntryContent);

                        LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

                        levelEntryData.id = i;
                        levelEntryData.levelName = response.assets[i].name;

                        if (playerID != string.Empty)
                        {
                            if (response.assets[i].asset_candidate.created_by_player_id.ToString() == playerIDResponse.payload.value)
                            {
                                levelEntryData.EnableDeleteButton();
                            }
                        }

                        for (int ii = 0; ii < response.assets[i].storage.Length; ii++)
                        {
                            if (response.assets[i].storage[ii].key == "assetID")
                            {
                                levelEntryData.assetID = response.assets[i].storage[ii].value;
                            }
                        }

                        LootLockerFile[] levelImageFiles = response.assets[i].files;

                        StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));

                        levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
                    }
                });
            }
        }, null, true);
    }

    IEnumerator LoadLevelIcon(string _imageURl, Image _levelImage)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_imageURl);

        yield return www.SendWebRequest();

        Texture2D loadedImage = DownloadHandlerTexture.GetContent(www);

        _levelImage.sprite = Sprite.Create(loadedImage, new Rect(0f, 0f, loadedImage.width, loadedImage.height), Vector2.zero);
    }

    private void StartAdded(int _amount)
    {
        startPointAdded += _amount;
    }

    private void FinishAdded(int _amount)
    {
        finishPointAdded += _amount;
    }

    private void LevelCompleted()
    {
        levelCompletedUI.SetActive(true);
    }

    public void RestartGame()
    {
        levelCompletedUI.SetActive(false);

        LoadLevel();

        StartGame();
    }

    public void EditGame()
    {
        EventSystemNew.RaiseEvent(Event_Type.EDIT_LEVEL);

        levelCompletedUI.SetActive(false);

        LoadLevel();

        editorUI.SetActive(true);

        inGameUI.SetActive(false);
    }

    public void StartGame()
    {
        if (startPointAdded > 0 && finishPointAdded > 0)
        {
            SaveLevel();

            EventSystemNew.RaiseEvent(Event_Type.GAME_STARTED);

            EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

            editorUI.SetActive(false);

            inGameUI.SetActive(true);
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);
        }
    }

    public void SetRetrieveOwnLevelsOnly(bool _retrieveOwnOnly)
    {
        retrieveOwnLevelsOnly = !_retrieveOwnOnly;

        DownloadLevelData();
    }

    private void DeactiveLevel(string _levelName, string _assetID)
    {
        LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
        {
            for (int i = 0; i < response.assets.Length; i++)
            {
                if (_levelName == response.assets[i].name)
                {
                    for (int ii = 0; ii < response.assets[i].storage.Length; ii++)
                    {
                        if (response.assets[i].storage[ii].key == "assetID")
                        {
                            if (response.assets[i].storage[ii].value == _assetID)
                            {
                                string textFilePath = Application.dataPath + "/" + "Inactive.abc";
                                if (!Directory.Exists(textFilePath))
                                {
                                    File.Create(Application.dataPath + "/" + "Inactive.abc").Dispose();
                                }

                                LootLocker.LootLockerEnums.FilePurpose textFileType = LootLocker.LootLockerEnums.FilePurpose.file;

                                LootLockerSDKManager.AddingFilesToAssetCandidates(int.Parse(_assetID), textFilePath, "Inactive.abc", textFileType, (response) =>
                                {
                                    if (response.success)
                                    {
                                        Debug.Log("ADDED FILE");
                                        for (int i = 0; i < response.asset_candidate.files.Length; i++)
                                        {
                                            Debug.Log("File Name: " + response.asset_candidate.files[i].name + "ID: " + response.asset_candidate.files[i].id);
                                        }

                                        //LootLockerSDKManager.RemovingFilesFromAssetCandidates(int.Parse(_assetID), 1, (response2) =>
                                        //{
                                        //    if (response2.success)
                                        //    {
                                        //        Debug.Log("Removed File");
                                        //    }
                                        //});
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }, null, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class AllLevelData
{
    public PrefabLevelData prefabLevelData;
    public TileLevelData tileLevelData;
}
