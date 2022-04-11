using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemController : MonoBehaviour
{
    [SerializeField] LevelManagerType levelManagerType;

    [SerializeField] GameObject itemDrawer;

    [SerializeField] GameObject editButton, stopEditButton;

    [SerializeField] GameObject hoverText;

    [SerializeField] GameObject[] dragImages;

    [SerializeField] Sprite[] itemImages;

    [SerializeField] GameObject[] items;

    [SerializeField] CustomTile[] customTiles;

    [SerializeField] Vector2[] itemImageCustomSizes;

    [SerializeField] Image activeImage;

    GameObject activeDragImage;

    int activeItemID = 0;

    private void OnEnable()
    {
        EventSystemNew<float>.Subscribe(Event_Type.ROTATE_ITEM, RotateItem);

        EventSystemNew.Subscribe(Event_Type.DESTROY_DRAG_IMAGE, DestroyDragImage);
    }

    private void OnDisable()
    {
        EventSystemNew<float>.Unsubscribe(Event_Type.ROTATE_ITEM, RotateItem);

        EventSystemNew.Unsubscribe(Event_Type.DESTROY_DRAG_IMAGE, DestroyDragImage);
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

    public void SelectItem(int _itemID)
    {
        StopEditItem();

        activeItemID = _itemID;

        if (levelManagerType == LevelManagerType.PrefabManager)
        {
            EventSystemNew<GameObject>.RaiseEvent(Event_Type.EQUIP_PREFAB, items[_itemID]);
        }

        if (levelManagerType == LevelManagerType.TileManager)
        {
            EventSystemNew<CustomTile>.RaiseEvent(Event_Type.EQUIP_TILE, customTiles[_itemID]);
        }

        activeImage.sprite = itemImages[_itemID];

        activeImage.rectTransform.sizeDelta = itemImageCustomSizes[_itemID];

        EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

        InstantiateImage(_itemID);

        EventSystemNew<LevelManagerType>.RaiseEvent(Event_Type.ENABLE_LEVEL_EDITOR, levelManagerType);
    }

    public void ActivateItem()
    {
        StopEditItem();

        if (levelManagerType == LevelManagerType.PrefabManager)
        {
            EventSystemNew<GameObject>.RaiseEvent(Event_Type.EQUIP_PREFAB, items[activeItemID]);
        }

        if (levelManagerType == LevelManagerType.TileManager)
        {
            EventSystemNew<CustomTile>.RaiseEvent(Event_Type.EQUIP_TILE, customTiles[activeItemID]);
        }

        EventSystemNew.RaiseEvent(Event_Type.DESTROY_DRAG_IMAGE);

        InstantiateImage(activeItemID);

        EventSystemNew<LevelManagerType>.RaiseEvent(Event_Type.ENABLE_LEVEL_EDITOR, levelManagerType);
    }

    private void InstantiateImage(int _itemID)
    {
        if (activeDragImage != dragImages[_itemID])
        {
            if (activeDragImage != null)
                Destroy(activeDragImage);

            activeDragImage = Instantiate(dragImages[_itemID], new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0), Quaternion.identity);
        }
    }

    private void RotateItem(float _rotation)
    {
        activeDragImage.transform.rotation = Quaternion.Euler(0f, 0f, _rotation);
    }

    private void DestroyDragImage()
    {
        if (activeDragImage != null)
        {
            Destroy(activeDragImage);
        }
    }
}
