using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;

    public void StopItemSelection()
    {
        EventSystemNew.RaiseEvent(Event_Type.STOP_ITEMS);
    }

    public void StopMoving()
    {
        EventSystemNew<bool>.RaiseEvent(Event_Type.TOGGLE_DRAGGING, false);
    }

    public void LoadScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DeselectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
