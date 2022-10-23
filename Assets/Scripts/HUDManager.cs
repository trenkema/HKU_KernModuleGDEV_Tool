using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [SerializeField] GameObject _editorUI;
    public GameObject editorUI { get { return _editorUI; } }

    [SerializeField] GameObject _inGameUI;
    public GameObject inGameUI { get { return _inGameUI; } }

    [SerializeField] GameObject _modifyingLevelBox;
    public GameObject modifyingLevelBox { get { return _modifyingLevelBox; } }

    [SerializeField] GameObject _loadingLevelText;
    public GameObject loadingLevelText { get { return _loadingLevelText; } }

    [SerializeField] GameObject _uploadingLevelText;
    public GameObject uploadingLevelText { get { return _uploadingLevelText; } }

    [SerializeField] GameObject _updatingLevelText;
    public GameObject updatingLevelText { get { return _updatingLevelText; } }

    [SerializeField] GameObject _downloadingLevelsUI;
    public GameObject downloadingLevelsUI { get { return _downloadingLevelsUI; } }

    [SerializeField] GameObject _updateLevelUI;
    public GameObject updateLevelUI { get { return _updateLevelUI; } }

    [SerializeField] GameObject _levelUploadUI;
    public GameObject levelUploadUI { get { return _levelUploadUI; } }

    [SerializeField] GameObject _downloadLevelUI;
    public GameObject downloadLevelUI { get { return _downloadLevelUI; } }

    [SerializeField] GameObject _failedToLoadUI;
    public GameObject failedToLoadUI { get { return _failedToLoadUI; } }

    [SerializeField] GameObject _levelCompletedUI;
    public GameObject levelCompletedUI { get { return _levelCompletedUI; } }

    [SerializeField] GameObject _gameOverUI;
    public GameObject gameOverUI { get { return _gameOverUI; } }

    [SerializeField] GameObject _manualScreenshotUI;
    public GameObject manualScreenshotUI { get { return _manualScreenshotUI; } }

    [SerializeField] GameObject _timerUI;
    public GameObject timerUI { get { return _timerUI; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}
