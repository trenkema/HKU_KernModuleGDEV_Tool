using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemController : MonoBehaviour
{
    [SerializeField] int itemControllerID = 0;

    [SerializeField] LevelManagerType levelManagerType;

    [SerializeField] GameObject itemDrawer;

    [SerializeField] GameObject editButton, stopEditButton;

    [SerializeField] GameObject hoverText;

    [SerializeField] Item[] items;

    [SerializeField] Image activeImage;

    GameObject activeDragImage;

    int activeItemID = 0;

    bool isActive = false;

    private void OnEnable()
    {
        EventSystemNew<float>.Subscribe(Event_Type.ROTATE_ITEM, RotateItem);

        EventSystemNew.Subscribe(Event_Type.DESTROY_DRAG_IMAGE, DestroyDragImage);

        EventSystemNew<int>.Subscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, ActivateItemController);

        EventSystemNew.Subscribe(Event_Type.STOP_ITEMS, StopItem);

        EventSystemNew<bool>.Subscribe(Event_Type.DRAGGING, DraggingLevel);
    }

    private void OnDisable()
    {
        EventSystemNew<float>.Unsubscribe(Event_Type.ROTATE_ITEM, RotateItem);

        EventSystemNew.Unsubscribe(Event_Type.DESTROY_DRAG_IMAGE, DestroyDragImage);

        EventSystemNew<int>.Unsubscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, ActivateItemController);

        EventSystemNew.Unsubscribe(Event_Type.STOP_ITEMS, StopItem);

        EventSystemNew<bool>.Unsubscribe(Event_Type.DRAGGING, DraggingLevel);
    }

    public void OnHoverEnter()
    {
        hoverText.SetActive(true);
    }

    public void OnHoverExit()
    {
        hoverText.SetActive(false);
    }

    public void EditItem()
    {
        itemDrawer.SetActive(true);

        editButton.SetActive(false);

        stopEditButton.SetActive(true);
    }

    public void StopEditItem()
    {
        itemDrawer.SetActive(false);

        editButton.SetActive(true);

        stopEditButton.SetActive(false);
    }

    private void StopItem()
    {
        isActive = false;

        EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);
        EventSystemNew<LevelManagerType>.RaiseEvent(Event_Type.ENABLE_LEVEL_EDITOR, LevelManagerType.None);
    }

    public void SelectItem(int _itemID)
    {
        EventSystemNew<int>.RaiseEvent(Event_Type.ACTIVATE_ITEM_CONTROLLER, itemControllerID);

        StopEditItem();

        activeItemID = _itemID;

        if (levelManagerType == LevelManagerType.PrefabManager)
        {
            EventSystemNew<GameObject>.RaiseEvent(Event_Type.EQUIP_PREFAB, items[_itemID].prefab);
        }

        if (levelManagerType == LevelManagerType.TileManager)
        {
            EventSystemNew<CustomTile>.RaiseEvent(Event_Type.EQUIP_TILE, items[_itemID].customTile);
        }

        activeImage.sprite = items[_itemID].itemImage;

        activeImage.rectTransform.sizeDelta = items[_itemID].itemImageSize;

        EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

        InstantiateImage(_itemID);

        EventSystemNew<LevelManagerType>.RaiseEvent(Event_Type.ENABLE_LEVEL_EDITOR, levelManagerType);
    }

    public void ActivateItem()
    {
        SelectItem(activeItemID);
    }

    private void InstantiateImage(int _itemID)
    {
        if (activeDragImage != items[_itemID].dragImage)
        {
            if (activeDragImage != null)
                Destroy(activeDragImage);

            activeDragImage = Instantiate(items[_itemID].dragImage, new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0), Quaternion.identity);
        }
    }

    private void RotateItem(float _rotation)
    {
        if (isActive)
        {
            activeDragImage.transform.rotation = Quaternion.Euler(0f, 0f, _rotation);
        }
    }

    private void DestroyDragImage()
    {
        if (activeDragImage != null)
        {
            Destroy(activeDragImage);
        }
    }

    private void ActivateItemController(int _itemControllerID)
    {
        if (itemControllerID == _itemControllerID)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
    }

    private void DraggingLevel(bool _isDragging)
    {
        if (activeDragImage != null)
        {
            if (_isDragging && isActive)
            {
                activeDragImage.SetActive(false);
            }
            else if (!_isDragging && isActive)
            {
                activeDragImage.SetActive(true);
            }
        }
    }
}
