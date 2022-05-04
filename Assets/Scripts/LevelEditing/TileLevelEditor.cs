using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TileLevelEditor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Tilemap defaultTilemap;

    [SerializeField] Camera cam;

    [Header("Settings")]
    [SerializeField] LevelManagerType levelManagerType;

    Tilemap currentTilemap
    {
        get
        {
            if (TileLevelManager.Instance.layers.TryGetValue((int)TileLevelManager.Instance.tiles[selectedTileIndex].tilemap, out Tilemap tilemap))
            {
                return tilemap;
            }
            else
            {
                return defaultTilemap;
            }
        }
    }

    TileBase currentTile
    {
        get
        {
            return TileLevelManager.Instance.tiles[selectedTileIndex].tile;
        }
    }
    
    int selectedTileIndex = 0;

    bool isPlacing = false;

    bool isDeleting = false;

    bool isActive = false;

    bool isHovering = false;

    private void OnEnable()
    {
        EventSystemNew<CustomTile>.Subscribe(Event_Type.EQUIP_TILE, EquipTile);
        EventSystemNew<LevelManagerType>.Subscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);

        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew<CustomTile>.Subscribe(Event_Type.EQUIP_TILE, EquipTile);
        EventSystemNew<LevelManagerType>.Unsubscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);

        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void Update()
    {        
        if (isPlacing && !isHovering && isActive)
        {
            if (!GraphicRaycasterCheck.Instance.IsHittingUI())
            {
                Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

                PlaceTile(pos);
            }
        }

        if (isDeleting && !isHovering && isActive)
        {
            if (!GraphicRaycasterCheck.Instance.IsHittingUI())
            {
                if (currentTilemap.name != Tilemaps.Background.ToString())
                {
                    Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

                    DeleteTile(pos);
                }
            }
        }
    }

    private void GameStarted()
    {
        isActive = false;
    }

    private void EnableLevelEditor(LevelManagerType _levelManagerType)
    {
        if (levelManagerType == _levelManagerType)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
    }

    public void OnPlaceTile(InputAction.CallbackContext _callbackContext)
    {
        if (isActive && !isHovering)
        {
            if (_callbackContext.phase == InputActionPhase.Started)
            {
                isPlacing = true;
            }
            else if (_callbackContext.phase == InputActionPhase.Canceled)
            {
                isPlacing = false;
            }
        }
    }

    public void OnDeleteTile(InputAction.CallbackContext _callbackContext)
    {
        if (isActive && !isHovering)
        {
            if (_callbackContext.phase == InputActionPhase.Performed)
            {
                isDeleting = true;
            }
            else if (_callbackContext.phase == InputActionPhase.Canceled)
            {
                isDeleting = false;
            }
        }
    }

    private void EquipTile(CustomTile _customTile)
    {
        for (int i = 0; i < TileLevelManager.Instance.tiles.Count; i++)
        {
            if (TileLevelManager.Instance.tiles[i] == _customTile)
            {
                selectedTileIndex = i;
            }
        }
    }

    void PlaceTile(Vector3Int _pos)
    {
        currentTilemap.SetTile(_pos, currentTile);
    }

    void DeleteTile(Vector3Int _pos)
    {
        currentTilemap.SetTile(_pos, null);
    }
}
