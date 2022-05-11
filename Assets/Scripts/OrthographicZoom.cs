using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrthographicZoom : MonoBehaviour
{
    [SerializeField] GameObject hoverText;

    [SerializeField] GameObject crossIcon;

    [SerializeField] Camera cam;

    [SerializeField] float maxZoom = 5f;

    [SerializeField] float minZoom = 20f;

    [SerializeField] float sensitivity = 1f;

    [SerializeField] float speed = 25f;

    [SerializeField] float minX, maxX, minY, maxY;

    [SerializeField] bool clampPosition = true;

    Vector3 dragOrigin;

    float targetZoom;

    bool canDrag = true;

    bool isDragging = false;

    float startZoom;

    private void OnEnable()
    {
        EventSystemNew<bool>.Subscribe(Event_Type.TOGGLE_DRAGGING, ToggleDrag);
    }

    private void OnDisable()
    {
        EventSystemNew<bool>.Unsubscribe(Event_Type.TOGGLE_DRAGGING, ToggleDrag);
    }

    private void Start()
    {
        startZoom = cam.orthographicSize;
    }

    private void Update()
    {
        targetZoom -= Input.mouseScrollDelta.y * sensitivity;
        targetZoom = Mathf.Clamp(targetZoom, maxZoom, minZoom);
        float newSize = Mathf.MoveTowards(cam.orthographicSize, targetZoom, speed * Time.deltaTime);
        cam.orthographicSize = newSize;

        cam.gameObject.transform.position = ClampCamera(cam.gameObject.transform.position);

        if (canDrag && isDragging)
        {
            Debug.Log("Panning Camera");

            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            cam.gameObject.transform.position = ClampCamera(cam.gameObject.transform.position + difference);
        }
    }

    public void OnHoverEnter()
    {
        hoverText.SetActive(true);
    }

    public void OnHoverExit()
    {
        hoverText.SetActive(false);
    }

    public void PanClickedCamera(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Debug.Log("Can Drag");

            isDragging = true;

            dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            Debug.Log("Can't Drag");

            isDragging = false;
        }
    }

    private Vector3 ClampCamera(Vector3 _targetPosition)
    {
        if (clampPosition)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;

            float newMinX = minX + camWidth;
            float newMaxX = maxX - camWidth;
            float newMinY = minY + camHeight;
            float newMaxY = maxY - camHeight;

            float newX = Mathf.Clamp(_targetPosition.x, newMinX, newMaxX);
            float newY = Mathf.Clamp(_targetPosition.y, newMinY, newMaxY);

            return new Vector3(newX, newY, -10f);
        }

        return new Vector3(_targetPosition.x, _targetPosition.y, -10f);
    }

    public void ToggleDrag()
    {
        canDrag = !canDrag;

        crossIcon.SetActive(canDrag ? false : true);

        if (canDrag)
        {
            EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);
            EventSystemNew<int>.RaiseEvent(Event_Type.ACTIVATE_ITEM_CONTROLLER, -1);
            EventSystemNew<LevelManagerType>.RaiseEvent(Event_Type.ENABLE_LEVEL_EDITOR, LevelManagerType.None);
        }
    }

    public void ToggleDrag(bool _canDrag)
    {
        canDrag = _canDrag;

        crossIcon.SetActive(canDrag ? false : true);
    }

    public void ResetCamera()
    {
        cam.orthographicSize = startZoom;

        cam.gameObject.transform.position = new Vector3(0f, 0f, -10f);
    }
}
