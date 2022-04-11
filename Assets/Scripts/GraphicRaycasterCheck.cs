using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GraphicRaycasterCheck : MonoBehaviour
{
    public static GraphicRaycasterCheck Instance;

    [SerializeField] GraphicRaycaster graphicRaycaster;
    [SerializeField] EventSystem eventSystem;

    PointerEventData pointerEventData;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public bool IsHittingUI()
    {
        pointerEventData = new PointerEventData(eventSystem);

        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        graphicRaycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            return true;
        }

        return false;
    }
}
