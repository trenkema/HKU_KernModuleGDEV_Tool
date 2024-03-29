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
using Assets.SimpleZip;
using UnityEngine.SceneManagement;

public enum LevelManagerType
{
    None,
    TileManager,
    PrefabManager
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] float restartEditCooldown = 1f;

    [SerializeField] AudioSource audioSource;

    [SerializeField] GameObject pageSelector;

    [SerializeField] GameObject updateLevelButton;

    [SerializeField] TMP_InputField levelNameInputField;

    [SerializeField] GameObject missingStartOrFinishUI;

    [SerializeField] GameObject startMissing, finishMissing;
    [SerializeField] TextMeshProUGUI tooManyStartsText, tooManyFinishesText;

    [SerializeField] Toggle updateLevelToggle;
    [SerializeField] Toggle uploadLevelToggle;

    [SerializeField] GameObject levelEntryDisplayItem;
    [SerializeField] Transform levelDataEntryContent;

    List<GameObject> displayItems = new List<GameObject>();

    int levelDatabaseID = 7943;
    int highscoreDatabaseID = 8667;
    string levelName;
    string playerName;
    int currentAssetID = -1;
    int startPointsAdded = 0;
    int finishPointsAdded = 0;
    bool retrieveOwnLevelsOnly = false;
    bool isPlaying = false;
    bool canRestartEdit = true;

    int pageIndex = 0;

    private void OnEnable()
    {
        EventSystemNew<bool>.Subscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);
        EventSystemNew<string, LevelEntryData>.Subscribe(Event_Type.LOAD_LEVEL_DATA, LoadLevelData);
        EventSystemNew<string>.Subscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);
        EventSystemNew<string>.Subscribe(Event_Type.ACTIVATE_LEVEL, ActivateLevel);
        EventSystemNew<int>.Subscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Subscribe(Event_Type.FINISH_ADDED, FinishAdded);
        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
        EventSystemNew.Subscribe(Event_Type.LEVEL_FAILED, LevelFailed);
    }

    private void OnDisable()
    {
        EventSystemNew<bool>.Unsubscribe(Event_Type.LOADING_SCREEN, SetLoadingScreen);
        EventSystemNew<string, LevelEntryData>.Unsubscribe(Event_Type.LOAD_LEVEL_DATA, LoadLevelData);
        EventSystemNew<string>.Unsubscribe(Event_Type.DEACTIVATE_LEVEL, DeactiveLevel);
        EventSystemNew<string>.Unsubscribe(Event_Type.ACTIVATE_LEVEL, ActivateLevel);
        EventSystemNew<int>.Unsubscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Unsubscribe(Event_Type.FINISH_ADDED, FinishAdded);
        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
        EventSystemNew.Unsubscribe(Event_Type.LEVEL_FAILED, LevelFailed);
    }

    private void Start()
    {
        pageSelector.SetActive(false);

        HUDManager.Instance.manualScreenshotUI.SetActive(false);

        updateLevelButton.SetActive(false);

        LootLockerSDKManager.GetPlayerName(response =>
        {
            if (!response.success)
            {
                HUDManager.Instance.failedToLoadUI.SetActive(true);
                return;
            }

            playerName = response.name;
        });
    }

    private void SetPlayerID(LootLockerAssetResponse _assets)
    {
        for (int i = 0; i < _assets.assets.Length; i++)
        {
            if (levelName == _assets.assets[i].name)
            {
                for (int ii = 0; ii < _assets.assets[i].storage.Length; ii++)
                {
                    if (_assets.assets[i].storage[ii].key == "assetID")
                    {
                        if (currentAssetID.ToString() == _assets.assets[i].storage[ii].value)
                        {
                            LootLockerSDKManager.UpdateOrCreateKeyValue("playerID", (_assets.assets[i].asset_candidate.created_by_player_id).ToString(), (getPersistentStorageResponse) =>
                            {
                            });
                        }
                    }
                }
            }
        }
    }

    private void GetPlayerStorage(System.Action<LootLockerPayload> OnDone)
    {
        LootLockerPayload payLoad = null;

        LootLockerSDKManager.GetSingleKeyPersistentStorage("playerID", (playerPayLoad) =>
        {
            if (playerPayLoad.success)
            {
                if (playerPayLoad.payload != null)
                {
                    payLoad = playerPayLoad.payload;
                }

                OnDone?.Invoke(payLoad);
            }
            else
            {
                Debug.Log("Storage Failed");

                OnDone?.Invoke(payLoad);
            }
        });
    }

    private void GetLevels(System.Action<LootLockerAssetResponse> OnDone)
    {
        LootLockerAssetResponse assetResponse = null;

        LootLockerSDKManager.GetAssetListWithCount(999999, (response) =>
        {
            if (response.success)
            {
                assetResponse = response;

                OnDone?.Invoke(assetResponse);
            }
            else
            {
                OnDone?.Invoke(assetResponse);
            }
        }, null, true);
    }

    private void InstantiateLevelButton(LootLockerCommonAsset _asset, int _index, bool _isActive, bool _ownLevelsOnly)
    {
        GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

        displayItems.Add(displayItem);

        displayItem.transform.SetParent(levelDataEntryContent);

        LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

        levelEntryData.id = _index;
        levelEntryData.levelName = _asset.name;

        if (_ownLevelsOnly)
        {
            if (_isActive)
            {
                levelEntryData.EnableDeleteButton();
            }
            else
            {
                levelEntryData.EnableActivateButton();
            }
        }

        for (int ii = 0; ii < _asset.storage.Length; ii++)
        {
            if (_asset.storage[ii].key == "assetID")
            {
                levelEntryData.assetID = _asset.storage[ii].value;
            }
        }

        LootLockerFile[] levelImageFiles = _asset.files;
        StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));
        levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
    }

    private void GetScoreboardLevels(LootLockerAssetResponse _assets, string _playerID)
    {
        LootLockerSDKManager.GetScoreList(levelDatabaseID, 2000, (levelListResponse) =>
        {
            if (levelListResponse.statusCode != 200)
            {
                return;
            }

            for (int i = 0; i < levelListResponse.items.Length; i++)
            {
                LootLockerCommonAsset asset = _assets.assets[levelListResponse.items[i].score];

                if (_playerID == "-1" && !retrieveOwnLevelsOnly)
                {
                    Debug.Log("New Account");

                    InstantiateLevelButton(asset, i, false, false);
                    continue;
                }

                if (asset.asset_candidate.created_by_player_id.ToString() == _playerID)
                {
                    bool isActive = levelListResponse.items[i].metadata == "-1" ? false : true;
                    InstantiateLevelButton(asset, i, isActive, true);
                }
                else if (asset.asset_candidate.created_by_player_id.ToString() != _playerID && levelListResponse.items[i].metadata != "-1" && !retrieveOwnLevelsOnly)
                {
                    InstantiateLevelButton(asset, i, false, false);
                }
            }

            Invoke(nameof(AssetsDownloaded), 0.3f);
        });
    }

    private void AddLevelToLeaderboard(LootLockerAssetResponse _assets, int _assetID, LootLockerUserGenerateContentResponse _assetCandidate)
    {
        for (int i = 0; i < _assets.assets.Length; i++)
        {
            if (_assets.assets[i].id == _assetCandidate.asset_candidate.asset_id)
            {
                LootLockerSDKManager.SubmitScore(_assetID.ToString(), i, levelDatabaseID, (scoreResponse) =>
                {
                    if (scoreResponse.statusCode != 200)
                    {
                        return;
                    }

                    updateLevelButton.SetActive(true);

                    HUDManager.Instance.modifyingLevelBox.SetActive(false);
                    HUDManager.Instance.uploadingLevelText.SetActive(false);
                    HUDManager.Instance.updatingLevelText.SetActive(false);

                    EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                    EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);
                });

                break;
            }
        }
    }

    private void IsLevelMine(string _playerID, string _assetID, LootLockerAssetResponse _assets)
    {
        LootLockerSDKManager.GetMemberRank(levelDatabaseID.ToString(), int.Parse(_assetID), (levelResponse) =>
        {
            if (levelResponse.statusCode != 200)
            {
                return;
            }

            for (int i = 0; i < _assets.assets.Length; i++)
            {
                if (i != levelResponse.score)
                {
                    continue;
                }

                LootLockerCommonAsset asset = _assets.assets[i];

                if (asset.asset_candidate.created_by_player_id.ToString() == _playerID)
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
        });
    }

    private void SetLoadingScreen(bool _isActive)
    {
        HUDManager.Instance.downloadLevelUI.SetActive(false);
        HUDManager.Instance.modifyingLevelBox.SetActive(_isActive);
        HUDManager.Instance.loadingLevelText.SetActive(_isActive);
    }

    private void LoadLevelData(string _textFileURL, LevelEntryData _levelData)
    {
        currentAssetID = int.Parse(_levelData.assetID);
        levelName = _levelData.levelName;

        StartCoroutine(DownloadLevelTextFile(_textFileURL, _levelData.assetID));
    }

    private IEnumerator DownloadLevelTextFile(string _textFileURL, string _assetID)
    {
        UnityWebRequest www = UnityWebRequest.Get(_textFileURL);

        yield return www.SendWebRequest();

        string filePath = Application.dataPath + "/" + "LevelData.txt";

        File.WriteAllText(filePath, www.downloadHandler.text);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        yield return new WaitForSeconds(0.1f);

        EventSystemNew.RaiseEvent(Event_Type.LOAD_LEVEL);

        EventSystemNew<string>.RaiseEvent(Event_Type.SET_LEVEL_ID, _assetID);

        // Check If You Own It
        LootLockerAssetResponse assetResponse = null;

        GetLevels((levelData) =>
        {
            assetResponse = levelData;

            if (assetResponse != null)
            {
                GetPlayerStorage((playerData) =>
                {
                    if (playerData == null)
                    {
                        EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, false);

                        EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                        EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);
                        return;
                    }

                    string playerID = playerData.value == null ? string.Empty : playerData.value;

                    if (playerID != string.Empty)
                    {
                        IsLevelMine(playerID, _assetID, assetResponse);
                    }
                    else
                    {
                        EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, false);

                        EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, true);
                        EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_ZOOM, true);
                    }
                });
            }
        });
    }

    public void CreateLevel()
    {
        HUDManager.Instance.modifyingLevelBox.SetActive(true);
        HUDManager.Instance.uploadingLevelText.SetActive(true);

        levelName = levelNameInputField.text;

        levelNameInputField.text = string.Empty;

        LootLockerSDKManager.CreatingAnAssetCandidate(levelName, (response) =>
        {
            if (response.success)
            {
                currentAssetID = response.asset_candidate_id;

                UploadLevelData(response.asset_candidate_id, response.asset_candidate_id, uploadLevelToggle.isOn ? true : false);
            }
        });
    }

    public void TakeScreenshot()
    {
        string filePath = Application.dataPath + "/";

        ScreenCapture.CaptureScreenshot(Path.Combine(filePath, "Level-Screenshot.png"));
    }

    IEnumerator WaitScreenshot(GameObject _uiToOpen)
    {
        TakeScreenshot();

        yield return new WaitForSeconds(0.25f);

        _uiToOpen.SetActive(true);
    }

    public void OpenUploadLevelUI()
    {
        if (startPointsAdded == 1 && finishPointsAdded == 1)
        {
            EventSystemNew.RaiseEvent(Event_Type.SAVE_LEVEL);

            StartCoroutine(WaitScreenshot(HUDManager.Instance.levelUploadUI));
        }
        else
        {
            CheckForStartFinish();
        }
    }

    public void OpenUpdateLevelUI()
    {
        if (startPointsAdded == 1 && finishPointsAdded == 1)
        {
            EventSystemNew.RaiseEvent(Event_Type.SAVE_LEVEL);

            StartCoroutine(WaitScreenshot(HUDManager.Instance.updateLevelUI));
        }
        else
        {
            CheckForStartFinish();
        }
    }

    private void CheckForStartFinish()
    {
        missingStartOrFinishUI.SetActive(true);

        startMissing.SetActive(startPointsAdded == 0 ? true : false);
        finishMissing.SetActive(finishPointsAdded == 0 ? true : false);

        tooManyStartsText.gameObject.SetActive(startPointsAdded > 1 ? true : false);
        tooManyFinishesText.gameObject.SetActive(finishPointsAdded > 1 ? true : false);

        tooManyStartsText.text = string.Format("- Remove {0} Start Points", startPointsAdded - 1);
        tooManyFinishesText.text = string.Format("- Remove {0} Finish Points", finishPointsAdded - 1);
    }

    public void UploadLevelData(int _levelID, int _assetID, bool _isPublic)
    {
        string screenshotFilePath = Application.dataPath + "/" + "Level-Screenshot.png";

        LootLocker.LootLockerEnums.FilePurpose screenshotFileType = LootLocker.LootLockerEnums.FilePurpose.primary_thumbnail;

        LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, screenshotFilePath, "Level-Screenshot.png", screenshotFileType, (screenshotResponse) =>
        {
            if (!screenshotResponse.success)
            {
                return;
            }

            string textFilePath = Application.dataPath + "/" + "LevelData.txt";

            LootLocker.LootLockerEnums.FilePurpose textFileType = LootLocker.LootLockerEnums.FilePurpose.file;

            LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, textFilePath, "LevelData.txt", textFileType, (textResponse) =>
            {
                if (!textResponse.success)
                {
                    return;
                }

                Dictionary<string, string> KV = new Dictionary<string, string>();
                KV.Add("playerName", playerName);
                KV.Add("assetID", _assetID.ToString());

                LootLockerSDKManager.UpdatingAnAssetCandidate(_levelID, true, (updatedResponse) =>
                    {
                        if (!updatedResponse.success)
                        {
                            return;
                        }

                        if (!_isPublic)
                        {
                            DeactiveLevel(_assetID.ToString());
                        }

                        LootLockerAssetResponse assetResponse = null;

                        GetLevels((levelData) =>
                        {
                            assetResponse = levelData;

                            if (assetResponse != null)
                            {
                                AddLevelToLeaderboard(assetResponse, _assetID, updatedResponse);

                                GetPlayerStorage((playerData) =>
                                {
                                    if (playerData == null)
                                    {
                                        SetPlayerID(assetResponse);
                                    }
                                });
                            }
                        });
                    }, null, KV);
            });
        });
    }

    public void DownloadLevelData()
    {
        pageSelector.SetActive(false);
        HUDManager.Instance.downloadingLevelsUI.SetActive(true);

        foreach (var displayItem in displayItems)
        {
            Destroy(displayItem);
        }

        displayItems.Clear();

        LootLockerAssetResponse assetResponse = null;

        GetLevels((data) =>
        {
            assetResponse = data;

            if (assetResponse != null)
            {
                GetPlayerStorage((playerData) =>
                {
                    if (playerData == null)
                    {
                        GetScoreboardLevels(assetResponse, "-1");
                        return;
                    }

                    string playerID = playerData.value == null ? string.Empty : playerData.value;

                    if (playerID != string.Empty)
                    {
                        GetScoreboardLevels(assetResponse, playerID);
                    }
                });
            }
        });
    }

    private void AssetsDownloaded()
    {
        pageSelector.SetActive(true);
        HUDManager.Instance.downloadingLevelsUI.SetActive(false);
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
        startPointsAdded += _amount;
    }

    private void FinishAdded(int _amount)
    {
        finishPointsAdded += _amount;
    }

    private void LevelCompleted()
    {
        HUDManager.Instance.levelCompletedUI.SetActive(true);
    }

    private void LevelFailed()
    {
        HUDManager.Instance.gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        if (isPlaying && canRestartEdit)
        {
            audioSource.time = 0;
            audioSource.Play();

            HUDManager.Instance.levelCompletedUI.SetActive(false);

            HUDManager.Instance.gameOverUI.SetActive(false);

            EventSystemNew.RaiseEvent(Event_Type.QUICK_LOAD_LEVEL);

            StartGame();
        }
    }

    public void QuickRestartGame(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started)
        {
            if (isPlaying && canRestartEdit)
            {
                audioSource.time = 0;
                audioSource.Play();

                HUDManager.Instance.levelCompletedUI.SetActive(false);

                HUDManager.Instance.gameOverUI.SetActive(false);

                EventSystemNew.RaiseEvent(Event_Type.QUICK_LOAD_LEVEL);

                StartGame();
            }
        }
    }

    public void EditGame()
    {
        if (isPlaying && canRestartEdit)
        {
            audioSource.Stop();

            isPlaying = false;

            HUDManager.Instance.levelCompletedUI.SetActive(false);

            HUDManager.Instance.gameOverUI.SetActive(false);

            EventSystemNew.RaiseEvent(Event_Type.LOAD_LEVEL);

            HUDManager.Instance.modifyingLevelBox.SetActive(false);
            HUDManager.Instance.loadingLevelText.SetActive(false);

            HUDManager.Instance.editorUI.SetActive(true);

            HUDManager.Instance.inGameUI.SetActive(false);
        }
    }

    public void EditGame(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started)
        {
            if (isPlaying && canRestartEdit)
            {
                EditGame();
            }
        }
    }

    public void StartGame()
    {
        if (startPointsAdded == 1 && finishPointsAdded == 1)
        {
            canRestartEdit = false;

            StartCoroutine(ResetRestartEditCooldown());

            audioSource.time = 0;
            audioSource.Play();

            Debug.Log("Started Game");

            isPlaying = true;

            EventSystemNew<bool>.RaiseEvent(Event_Type.LOADING_SCREEN, true);

            EventSystemNew.RaiseEvent(Event_Type.SAVE_LEVEL);

            EventSystemNew.RaiseEvent(Event_Type.GAME_STARTED);

            EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

            HUDManager.Instance.editorUI.SetActive(false);

            HUDManager.Instance.inGameUI.SetActive(true);
        }
        else
        {
            missingStartOrFinishUI.SetActive(true);

            startMissing.SetActive(startPointsAdded == 0 ? true : false);
            finishMissing.SetActive(finishPointsAdded == 0 ? true : false);


            tooManyStartsText.gameObject.SetActive(startPointsAdded > 1 ? true : false);
            tooManyFinishesText.gameObject.SetActive(finishPointsAdded > 1 ? true : false);

            tooManyStartsText.text = string.Format("- Remove {0} Start Points", startPointsAdded - 1);
            tooManyFinishesText.text = string.Format("- Remove {0} Finish Points", finishPointsAdded - 1);
        }
    }

    public void SetRetrieveOwnLevelsOnly(bool _retrieveOwnOnly)
    {
        retrieveOwnLevelsOnly = !_retrieveOwnOnly;

        DownloadLevelData();
    }

    public void UpdateLevel()
    {
        HUDManager.Instance.modifyingLevelBox.SetActive(true);
        HUDManager.Instance.updatingLevelText.SetActive(true);

        EventSystemNew.RaiseEvent(Event_Type.SAVE_LEVEL);

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

    public void TakeManualScreenshot()
    {
        StartCoroutine(ManualScreenshot());
    }

    private IEnumerator ManualScreenshot()
    {
        HUDManager.Instance.manualScreenshotUI.SetActive(false);

        TakeScreenshot();

        yield return new WaitForSeconds(0.25f);

        HUDManager.Instance.editorUI.SetActive(true);
    }

    private IEnumerator ResetRestartEditCooldown()
    {
        yield return new WaitForSeconds(restartEditCooldown);

        canRestartEdit = true;
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
