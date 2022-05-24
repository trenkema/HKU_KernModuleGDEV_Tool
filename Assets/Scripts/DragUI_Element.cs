using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragUI_Element : MonoBehaviour
{
    GameObject element;

    Camera cam;

    bool isDragging = false;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (isDragging)
        {
            element.transform.position = new Vector3(cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x, element.transform.position.y, element.transform.position.z);
        }
    }

    public void SetTarget(BaseEventData _eventData)
    {
        element = _eventData.selectedObject;

        isDragging = true;
    }

    public void RemoveTarget()
    {
        element = null;

        isDragging = false;
    }
}
