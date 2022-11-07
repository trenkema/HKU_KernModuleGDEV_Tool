using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] Image fullScreenButton;
    [SerializeField] Image windowedButton;

    [SerializeField] Color activeColor;

    private void Awake()
    {
        bool isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1 ? true : false;
        Screen.fullScreen = isFullScreen;

        SetButtonColors(isFullScreen);
    }

    public void ToggleFullscreen (bool _isFullScreen)
    {
        Screen.fullScreen = _isFullScreen;
        PlayerPrefs.SetInt("FullScreen", _isFullScreen ? 1 : 0);

        SetButtonColors(_isFullScreen);
    }

    private void SetButtonColors(bool _isFullscreen)
    {
        switch (_isFullscreen)
        {
            case true:
                fullScreenButton.color = activeColor;
                windowedButton.color = Color.white;
                break;
            case false:
                fullScreenButton.color = Color.white;
                windowedButton.color = activeColor;
                break;
        }
    }
}
