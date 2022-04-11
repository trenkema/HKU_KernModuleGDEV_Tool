using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class LevelManager : MonoBehaviour
{
    [SerializeField] TMP_InputField levelNameInputField;

    [SerializeField] GameObject levelUploadUI;

    [SerializeField] GameObject downloadLevelUI;

    [SerializeField] GameObject failedToLoadUI;

    [SerializeField] GameObject levelEntryDisplayItem;

    [SerializeField] Transform levelDataEntryContent;

    List<GameObject> displayItems = new List<GameObject>();

    string levelName;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.CLOSE_DOWNLOAD_MENU, CloseDownloadMenu);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.CLOSE_DOWNLOAD_MENU, CloseDownloadMenu);
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
        EventSystemNew.RaiseEvent(Event_Type.SAVE_LEVEL);

        StartCoroutine(WaitScreenshot());
    }

    public void UploadLevelData(int _levelID)
    {
        string screenshotFilePath = Application.dataPath + "/" + "Level-Screenshot.png";

        LootLocker.LootLockerEnums.FilePurpose screenshotFileType = LootLocker.LootLockerEnums.FilePurpose.primary_thumbnail;

        LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, screenshotFilePath, "Level-Screenshot.png", screenshotFileType, (screenshotResponse) =>
        {
            if (screenshotResponse.success)
            {
                string textFilePath = Application.dataPath + "/" + "prefabLevel.json";

                LootLocker.LootLockerEnums.FilePurpose textFileType = LootLocker.LootLockerEnums.FilePurpose.file;

                LootLockerSDKManager.AddingFilesToAssetCandidates(_levelID, textFilePath, "prefabLevel.json", textFileType, (textResponse) =>
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
