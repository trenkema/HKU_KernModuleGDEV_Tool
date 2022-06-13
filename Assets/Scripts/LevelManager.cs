using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine.InputSystem;

public enum LevelManagerType
{
    None,
    TileManager,
    PrefabManager
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    [SerializeField] GameObject pageSelector;

    [SerializeField] GameObject updateLevelButton;

    [SerializeField] TMP_InputField levelNameInputField;

    [SerializeField] GameObject missingStartOrFinishUI;

    [SerializeField] GameObject startMissing, finishMissing;
    [SerializeField] TextMeshProUGUI tooManyStartsText, tooManyFinishesText;

    [SerializeField] GameObject editorUI;
    [SerializeField] GameObject inGameUI;

    [SerializeField] GameObject loadingLevelUI;
    [SerializeField] GameObject uploadingLevelUI;
    [SerializeField] GameObject updatingLevelUI;
    [SerializeField] GameObject downloadingLevelsUI;

    [SerializeField] Toggle updateLevelToggle;
    [SerializeField] Toggle uploadLevelToggle;

    [SerializeField] GameObject updateLevelUI;

    [SerializeField] GameObject levelUploadUI;

    [SerializeField] GameObject downloadLevelUI;

    [SerializeField] GameObject failedToLoadUI;

    [SerializeField] GameObject levelCompletedUI;

    [SerializeField] GameObject gameOverUI;

    [SerializeField] GameObject manualScreenshotUI;

    [SerializeField] GameObject levelEntryDisplayItem;

    [SerializeField] Transform levelDataEntryContent;

    List<GameObject> displayItems = new List<GameObject>();

    int levelDatabaseID = 2881;

    string levelName;

    string playerName;

    int currentAssetID = -1;

    int startPointAdded = 0;
    int finishPointAdded = 0;

    bool retrieveOwnLevelsOnly = false;

    bool isPlaying = false;

    private void OnEnable()
    {
        EventSystemNew<bool>.Subscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);

        EventSystemNew<string, LevelEntryData>.Subscribe(Event_Type.LOAD_LEVEL, LoadLevelData);

        EventSystemNew<string>.Subscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);

        EventSystemNew<string>.Subscribe(Event_Type.ACTIVATE_LEVEL, ActivateLevel);

        EventSystemNew<string>.Subscribe(Event_Type.FAVORITE_LEVEL, FavoriteLevel);

        EventSystemNew<int>.Subscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Subscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);

        EventSystemNew.Subscribe(Event_Type.LEVEL_FAILED, LevelFailed);
    }

    private void OnDisable()
    {
        EventSystemNew<bool>.Unsubscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);

        EventSystemNew<string, LevelEntryData>.Unsubscribe(Event_Type.LOAD_LEVEL, LoadLevelData);

        EventSystemNew<string>.Unsubscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);

        EventSystemNew<string>.Unsubscribe(Event_Type.ACTIVATE_LEVEL, ActivateLevel);

        EventSystemNew<string>.Unsubscribe(Event_Type.FAVORITE_LEVEL, FavoriteLevel);

        EventSystemNew<int>.Unsubscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Unsubscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);

        EventSystemNew.Unsubscribe(Event_Type.LEVEL_FAILED, LevelFailed);
    }

    private void Start()
    {
        pageSelector.SetActive(false);

        manualScreenshotUI.SetActive(false);

        updateLevelButton.SetActive(false);

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

    private void LoadLevelData(string _textFileURL, LevelEntryData _levelData)
    {
        currentAssetID = int.Parse(_levelData.assetID);
        levelName = _levelData.levelName;

        StartCoroutine(DownloadLevelTextFile(_textFileURL, _levelData.assetID));
    }

    private IEnumerator DownloadLevelTextFile(string textFileURL, string _assetID)
    {
        UnityWebRequest www = UnityWebRequest.Get(textFileURL);

        yield return www.SendWebRequest();

        string filePath = Application.dataPath + "/" + "LevelData.json";

        File.WriteAllText(filePath, www.downloadHandler.text);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        yield return new WaitForSeconds(1f);

        string json = File.ReadAllText(Application.dataPath + "/LevelData.json");

        AllLevelData levelData = JsonUtility.FromJson<AllLevelData>(json);

        TileLevelManager.Instance.LoadLevel(levelData.tileLevelData);
        PrefabLevelEditor.Instance.LoadLevel(levelData.prefabLevelData);

        // Check If You Own It

        LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
        {
            if (response.success)
            {
                LootLockerSDKManager.GetSingleKeyPersistentStorage("playerID", (playerIDResponse) =>
                {
                    string playerID = "";

                    if (playerIDResponse.success)
                    {
                        if (playerIDResponse.payload != null)
                        {
                            playerID = playerIDResponse.payload.value;

                            LootLockerSDKManager.GetMemberRank(levelDatabaseID.ToString(), int.Parse(_assetID), (levelResponse) =>
                            {
                                if (levelResponse.statusCode == 200)
                                {
                                    for (int i = 0; i < response.assets.Length; i++)
                                    {
                                        if (i == levelResponse.score)
                                        {
                                            LootLockerCommonAsset asset = response.assets[i];

                                            if (asset.asset_candidate.created_by_player_id.ToString() == playerID)
                                            {
                                                updateLevelButton.SetActive(true);
                                            }
                                            else
                                            {
                                                updateLevelButton.SetActive(false);
                                            }

                                            EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, false);

                                            EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                                            EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, false);

                            EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                            EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);
                        }
                    }
                });
            }
        }, null, true);
    }

    public void CreateLevel()
    {
        uploadingLevelUI.SetActive(true);

        levelName = levelNameInputField.text;

        LootLockerSDKManager.CreatingAnAssetCandidate(levelName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Created Asset");

                currentAssetID = response.asset_candidate_id;

                UploadLevelData(response.asset_candidate_id, response.asset_candidate_id, uploadLevelToggle.isOn ? true : false);
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

        yield return new WaitForSeconds(0.25f);

        levelUploadUI.SetActive(true);
    }

    IEnumerator UpdateWaitScreenshot()
    {
        TakeScreenshot();

        yield return new WaitForSeconds(0.25f);

        updateLevelUI.SetActive(true);
    }

    public void OpenUploadLevelUI()
    {
        if (startPointAdded == 1 && finishPointAdded == 1)
        {
            SaveLevel();

            StartCoroutine(WaitScreenshot());
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);

            startMissing.SetActive(startPointAdded == 0 ? true : false);
            finishMissing.SetActive(finishPointAdded == 0 ? true : false);

            tooManyStartsText.gameObject.SetActive(startPointAdded > 1 ? true : false);
            tooManyFinishesText.gameObject.SetActive(finishPointAdded > 1 ? true : false);

            tooManyStartsText.text = string.Format("- Remove {0} Start Points", startPointAdded - 1);
            tooManyFinishesText.text = string.Format("- Remove {0} Finish Points", finishPointAdded - 1);
        }
    }

    public void OpenUpdateLevelUI()
    {
        if (startPointAdded == 1 && finishPointAdded == 1)
        {
            SaveLevel();

            StartCoroutine(UpdateWaitScreenshot());
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);

            startMissing.SetActive(startPointAdded == 0 ? true : false);
            finishMissing.SetActive(finishPointAdded == 0 ? true : false);

            tooManyStartsText.gameObject.SetActive(startPointAdded > 1 ? true : false);
            tooManyFinishesText.gameObject.SetActive(finishPointAdded > 1 ? true : false);

            tooManyStartsText.text = string.Format("- Remove {0} Start Points", startPointAdded - 1);
            tooManyFinishesText.text = string.Format("- Remove {0} Finish Points", finishPointAdded - 1);
        }
    }

    public void UploadLevelData(int _levelID, int _assetID, bool _isPublic)
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
                        KV.Add("assetID", _assetID.ToString());

                        LootLockerSDKManager.UpdatingAnAssetCandidate(_levelID, true, (updatedResponse) =>
                        {
                            if (updatedResponse.success)
                            {
                                LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
                                {
                                    for (int i = 0; i < response.assets.Length; i++)
                                    {
                                        if (response.assets[i].id == updatedResponse.asset_candidate.asset_id)
                                        {
                                            LootLockerSDKManager.SubmitScore(_assetID.ToString(), i, levelDatabaseID, (scoreResponse) =>
                                            {
                                                if (scoreResponse.statusCode == 200)
                                                {
                                                    Debug.Log("Successfully added level to LevelDatabase");

                                                    updateLevelButton.SetActive(true);

                                                    uploadingLevelUI.SetActive(false);

                                                    EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                                                    EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);

                                                    EventSystemNew<string>.RaiseEvent(Event_Type.DEACTIVATE_LEVEL, _assetID.ToString());
                                                }
                                                else
                                                {
                                                    Debug.Log("Failed to add Score: " + scoreResponse.Error);
                                                }
                                            });

                                            break;
                                        }
                                    }
                                }, null, true);

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
                                                                if (_assetID.ToString() == response.assets[i].storage[ii].value)
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
        pageSelector.SetActive(false);
        downloadingLevelsUI.SetActive(true);

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
                            LootLockerSDKManager.GetScoreList(levelDatabaseID, 2000, (levelListResponse) =>
                            {
                                if (levelListResponse.statusCode == 200)
                                {
                                    for (int i = 0; i < levelListResponse.items.Length; i++)
                                    {
                                        LootLockerCommonAsset asset = response.assets[levelListResponse.items[i].score];

                                        if (asset.asset_candidate.created_by_player_id.ToString() == playerIDResponse.payload.value)
                                        {
                                            GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

                                            displayItems.Add(displayItem);

                                            displayItem.transform.SetParent(levelDataEntryContent);

                                            LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

                                            levelEntryData.id = i;
                                            levelEntryData.levelName = asset.name;

                                            if (levelListResponse.items[i].metadata != "-1")
                                            {
                                                levelEntryData.EnableDeleteButton();
                                            }
                                            else
                                            {
                                                levelEntryData.EnableActivateButton();
                                            }

                                            for (int ii = 0; ii < asset.storage.Length; ii++)
                                            {
                                                if (asset.storage[ii].key == "assetID")
                                                {
                                                    levelEntryData.assetID = asset.storage[ii].value;
                                                }
                                            }

                                            LootLockerFile[] levelImageFiles = asset.files;

                                            StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));

                                            levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
                                        }
                                    }

                                    Invoke(nameof(AssetsDownloaded), 0.3f);
                                }
                                else
                                {
                                    Debug.Log("Failed to load level list: " + levelListResponse.Error);
                                }
                            });
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

                    LootLockerSDKManager.GetScoreList(levelDatabaseID, 2000, (levelListResponse) =>
                    {
                        if (levelListResponse.statusCode == 200)
                        {
                            for (int i = 0; i < levelListResponse.items.Length; i++)
                            {
                                LootLockerCommonAsset asset = response.assets[levelListResponse.items[i].score];

                                if (levelListResponse.items[i].metadata != "-1" || asset.asset_candidate.created_by_player_id.ToString() == playerID)
                                {
                                    GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

                                    displayItems.Add(displayItem);

                                    displayItem.transform.SetParent(levelDataEntryContent);

                                    LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

                                    levelEntryData.id = i;
                                    levelEntryData.levelName = asset.name;

                                    if (playerID != string.Empty)
                                    {
                                        if (asset.asset_candidate.created_by_player_id.ToString() == playerIDResponse.payload.value)
                                        {
                                            if (levelListResponse.items[i].metadata != "-1")
                                            {
                                                levelEntryData.EnableDeleteButton();
                                            }
                                            else
                                            {
                                                levelEntryData.EnableActivateButton();
                                            }
                                        }
                                    }

                                    for (int ii = 0; ii < asset.storage.Length; ii++)
                                    {
                                        if (asset.storage[ii].key == "assetID")
                                        {
                                            levelEntryData.assetID = asset.storage[ii].value;
                                        }
                                    }

                                    LootLockerFile[] levelImageFiles = asset.files;

                                    StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));

                                    levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
                                }
                            }

                            Invoke(nameof(AssetsDownloaded), 0.25f);
                        }
                        else
                        {
                            Debug.Log("Failed to load level list: " + levelListResponse.Error);
                        }
                    });
                });
            }
        }, null, true);
    }

    private void AssetsDownloaded()
    {
        pageSelector.SetActive(true);
        downloadingLevelsUI.SetActive(false);
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

    private void LevelFailed()
    {
        gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        if (isPlaying)
        {
            audioSource.time = 0;
            audioSource.Play();

            levelCompletedUI.SetActive(false);

            gameOverUI.SetActive(false);

            LoadLevel();

            StartGame();
        }
    }

    public void QuickRestartGame(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started)
        {
            if (isPlaying)
            {
                audioSource.time = 0;
                audioSource.Play();

                levelCompletedUI.SetActive(false);

                gameOverUI.SetActive(false);

                LoadLevel();

                StartGame();
            }
        }
    }

    public void EditGame()
    {
        if (isPlaying)
        {
            audioSource.Stop();

            isPlaying = false;

            levelCompletedUI.SetActive(false);

            gameOverUI.SetActive(false);

            LoadLevel();

            loadingLevelUI.SetActive(false);

            editorUI.SetActive(true);

            inGameUI.SetActive(false);
        }
    }

    public void EditGame(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started)
        {
            if (isPlaying)
            {
                audioSource.Stop();

                isPlaying = false;

                levelCompletedUI.SetActive(false);

                gameOverUI.SetActive(false);

                LoadLevel();

                loadingLevelUI.SetActive(false);

                editorUI.SetActive(true);

                inGameUI.SetActive(false);
            }
        }
    }

    public void StartGame()
    {
        if (startPointAdded == 1 && finishPointAdded == 1)
        {
            audioSource.time = 0;
            audioSource.Play();

            Debug.Log("Started Game");

            isPlaying = true;

            EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, true);

            SaveLevel();

            EventSystemNew.RaiseEvent(Event_Type.GAME_STARTED);

            EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

            editorUI.SetActive(false);

            inGameUI.SetActive(true);
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);

            startMissing.SetActive(startPointAdded == 0 ? true : false);
            finishMissing.SetActive(finishPointAdded == 0 ? true : false);


            tooManyStartsText.gameObject.SetActive(startPointAdded > 1 ? true : false);
            tooManyFinishesText.gameObject.SetActive(finishPointAdded > 1 ? true : false);

            tooManyStartsText.text = string.Format("- Remove {0} Start Points", startPointAdded - 1);
            tooManyFinishesText.text = string.Format("- Remove {0} Finish Points", finishPointAdded - 1);
        }
    }

    public void SetRetrieveOwnLevelsOnly(bool _retrieveOwnOnly)
    {
        retrieveOwnLevelsOnly = !_retrieveOwnOnly;

        DownloadLevelData();
    }

    public void UpdateLevel()
    {
        updatingLevelUI.SetActive(true);

        SaveLevel();

        LootLockerSDKManager.CreatingAnAssetCandidate(levelName, (response) =>
        {
            if (response.success)
            {
                UploadLevelData(response.asset_candidate_id, currentAssetID, updateLevelToggle.isOn ? true : false);
            }
        });
    }

    private void DeactiveLevel(string _assetID)
    {
        LootLockerSDKManager.GetMemberRank(levelDatabaseID.ToString(), int.Parse(_assetID), (response) =>
        {
            if (response.statusCode == 200)
            {
                LootLockerSDKManager.SubmitScore(_assetID, response.score, levelDatabaseID.ToString(), "-1", (scoreResponse) =>
                {
                    if (scoreResponse.statusCode == 200)
                    {
                        Debug.Log("Successfully deactivated level");

                        DownloadLevelData();
                    }
                    else
                    {
                        Debug.Log("Failed to deactivate level: " + scoreResponse.Error);
                    }
                });
            }
        });
    }

    private void ActivateLevel(string _assetID)
    {
        LootLockerSDKManager.GetMemberRank(levelDatabaseID.ToString(), int.Parse(_assetID), (response) =>
        {
            if (response.statusCode == 200)
            {
                if (response.metadata == "-1")
                {
                    LootLockerSDKManager.SubmitScore(_assetID, response.score, levelDatabaseID.ToString(), "0", (scoreResponse) =>
                    {
                        if (scoreResponse.statusCode == 200)
                        {
                            Debug.Log("Successfully activated level");

                            DownloadLevelData();
                        }
                        else
                        {
                            Debug.Log("Failed to activate level: " + scoreResponse.Error);
                        }
                    });
                }
            }
        });
    }

    private void FavoriteLevel(string _assetID)
    {
        LootLockerSDKManager.AddFavouriteAsset(_assetID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Added to favorite");
            }
        });
    }

    public void TakeManualScreenshot()
    {
        StartCoroutine(ManualScreenshot());
    }

    private IEnumerator ManualScreenshot()
    {
        manualScreenshotUI.SetActive(false);

        TakeScreenshot();

        yield return new WaitForSeconds(0.25f);

        editorUI.SetActive(true);
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
