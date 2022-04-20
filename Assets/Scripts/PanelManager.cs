using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    [SerializeField] GameObject[] panels;

    // Open Panel Function
    public void OpenPanel(GameObject Panel)
    {
        Panel.SetActive(true);
    }

    // Close Panel Function
    public void ClosePanel(GameObject Panel)
    {
        Panel.SetActive(false);
    }

    // Toggle Panel Function
    public void TogglePanel(GameObject Panel)
    {
        Panel.SetActive(!Panel.activeInHierarchy);
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }
}
