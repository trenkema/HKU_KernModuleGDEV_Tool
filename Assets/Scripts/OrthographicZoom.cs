using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrthographicZoom : MonoBehaviour
{
    [SerializeField] Camera cam;

    [SerializeField] float maxZoom = 5f;

    [SerializeField] float minZoom = 20f;

    [SerializeField] float sensitivity = 1f;

    [SerializeField] float speed = 25f;

    [SerializeField] float minX, maxX, minZ, maxZ;

    [SerializeField] bool clampPosition = true;

    Vector3 dragOrigin;

    float targetZoom;

    bool canDrag = true;

    bool isDragging = false;

    float startZoom;

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

        if (canDrag && isDragging)
        {
            Debug.Log("Panning Camera");

            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            cam.gameObject.transform.position = ClampCamera(cam.gameObject.transform.position + difference);
        }
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
            float newMinZ = minZ + camHeight;
            float newMaxZ = maxZ - camHeight;

            float newX = Mathf.Clamp(_targetPosition.x, newMinX, newMaxX);
            float newZ = Mathf.Clamp(_targetPosition.z, newMinZ, newMaxZ);

            return new Vector3(newX, newZ, -10f);
        }

        return new Vector3(_targetPosition.x, _targetPosition.y, -10f);
    }

    public void ToggleDrag()
    {
        canDrag = !canDrag;
    }

    public void ResetCamera()
    {
        cam.orthographicSize = startZoom;

        cam.gameObject.transform.position = new Vector3(0f, 0f, -10f);
    }
}
