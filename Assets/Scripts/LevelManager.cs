using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

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

    [SerializeField] GameObject levelUploadUI;

    [SerializeField] GameObject downloadLevelUI;

    [SerializeField] GameObject failedToLoadUI;

    [SerializeField] GameObject levelCompletedUI;

    [SerializeField] GameObject levelEntryDisplayItem;

    [SerializeField] Transform levelDataEntryContent;

    List<GameObject> displayItems = new List<GameObject>();

    string levelName;

    int startPointAdded = 0;
    int finishPointAdded = 0;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.CLOSE_DOWNLOAD_MENU, CloseDownloadMenu);

        EventSystemNew.Subscribe(Event_Type.LOAD_LEVEL, LoadLevel);

        EventSystemNew<int>.Subscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Subscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.CLOSE_DOWNLOAD_MENU, CloseDownloadMenu);

        EventSystemNew.Unsubscribe(Event_Type.LOAD_LEVEL, LoadLevel);

        EventSystemNew<int>.Unsubscribe(Event_Type.START_ADDED, StartAdded);
        EventSystemNew<int>.Unsubscribe(Event_Type.FINISH_ADDED, FinishAdded);

        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void Start()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Started Session");
            }
            else
            {
                Debug.Log(response.Error);

                Debug.Log("Failed Starting Session");

                failedToLoadUI.SetActive(true);
            }
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

    private void LoadLevel()
    {
        string json = File.ReadAllText(Application.dataPath + "/LevelData.json");

        AllLevelData levelData = JsonUtility.FromJson<AllLevelData>(json);

        TileLevelManager.Instance.LoadLevel(levelData.tileLevelData);
        PrefabLevelEditor.Instance.LoadLevel(levelData.prefabLevelData);
    }

    private void CloseDownloadMenu()
    {
        downloadLevelUI.SetActive(false);
    }

    public void CreateLevel()
    {
        levelName = levelNameInputField.text;

        LootLockerSDKManager.CreatingAnAssetCandidate(levelName, (response) =>
        {
            if (response.success)
            {
                UploadLevelData(response.asset_candidate_id);
            }
            else
            {
                Debug.Log("Error asset candidate");
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

    public void UploadLevelData(int _levelID)
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
                        LootLockerSDKManager.UpdatingAnAssetCandidate(_levelID, true, (updatedResponse) =>
                        {

                        });
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

        LootLockerSDKManager.GetAssetListWithCount(10, (response) =>
        {
            for (int i = 0; i < response.assets.Length; i++)
            {
                GameObject displayItem = Instantiate(levelEntryDisplayItem, transform.position, Quaternion.identity);

                displayItems.Add(displayItem);

                displayItem.transform.SetParent(levelDataEntryContent);

                LevelEntryData levelEntryData = displayItem.GetComponent<LevelEntryData>();

                levelEntryData.id = i;
                levelEntryData.levelName = response.assets[i].name;

                LootLockerFile[] levelImageFiles = response.assets[i].files;

                StartCoroutine(LoadLevelIcon(levelImageFiles[0].url.ToString(), levelEntryData.levelIcon));

                levelEntryData.textFileURL = levelImageFiles[1].url.ToString();
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
