using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/* Danndx 2021 (youtube.com/danndx)
From video: youtu.be/CnJtca6FgUs
thanks - delete me! :) */


public class SCR_UiDrag : MonoBehaviour
{
    public GameObject ui_canvas;
    GraphicRaycaster ui_raycaster;
    PointerEventData click_data;
    List<RaycastResult> click_results;

    List<GameObject> clicked_elements;

    bool dragging = false;
    GameObject drag_element;

    Vector3 mouse_position;
    Vector3 previous_mouse_position;

    void Start()
    {
        ui_raycaster = ui_canvas.GetComponent<GraphicRaycaster>();
        click_data = new PointerEventData(EventSystem.current);
        click_results = new List<RaycastResult>();
        clicked_elements = new List<GameObject>();
    }

    void Update()
    {
        MouseDragUi();
    }

    void MouseDragUi()
    {
        /** Houses the main mouse dragging logic. **/

        mouse_position = Mouse.current.position.ReadValue();

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectUi();
        }

        if(Mouse.current.leftButton.isPressed && dragging)
        {
            DragElement();
        }
        else
        {
            dragging = false;
        }

        previous_mouse_position = mouse_position;
    }

    void DetectUi()
    {
        /** Detect if the mouse has been clicked on a UI element, and begin dragging **/

        GetUiElementsClicked();

        if(clicked_elements.Count > 0)
        {
            dragging = true;
            drag_element = clicked_elements[0];
        }
    }

    void GetUiElementsClicked()
    {
        /** Get all the UI elements clicked, using the current mouse position and raycasting. **/

        click_data.position = mouse_position;
        click_results.Clear();
        ui_raycaster.Raycast(click_data, click_results);

        // Optimised version
        //clicked_elements = (from result in click_results select result.gameObject).ToList();

        // Foreach version
        clicked_elements.Clear();
        foreach(RaycastResult result in click_results)
        {
            clicked_elements.Add(result.gameObject);
        }

    }

    void DragElement()
    {
        /** Drag a UI element across the screen based on the mouse movement. **/

        RectTransform element_rect = drag_element.GetComponent<RectTransform>();

        Vector2 drag_movement = mouse_position - previous_mouse_position;
        element_rect.anchoredPosition = element_rect.anchoredPosition + drag_movement;
    }

}
