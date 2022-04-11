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

    public bool isActive = true;

    private void OnEnable()
    {
        EventSystemNew<int>.Subscribe(Event_Type.EQUIP_TILE, EquipTile);
        EventSystemNew<LevelManagerType>.Subscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);
    }

    private void OnDisable()
    {
        EventSystemNew<int>.Subscribe(Event_Type.EQUIP_TILE, EquipTile);
        EventSystemNew<LevelManagerType>.Unsubscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);
    }

    private void Update()
    {        
        if (isPlacing)
        {
            Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

            PlaceTile(pos);
        }

        if (isDeleting)
        {
            Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

            DeleteTile(pos);
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            selectedTileIndex++;

            if (selectedTileIndex >= TileLevelManager.Instance.tiles.Count)
            {
                selectedTileIndex = 0;
            }

            Debug.Log(TileLevelManager.Instance.tiles[selectedTileIndex].name);
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            selectedTileIndex--;

            if (selectedTileIndex < 0)
            {
                selectedTileIndex = TileLevelManager.Instance.tiles.Count - 1;
            }

            Debug.Log(TileLevelManager.Instance.tiles[selectedTileIndex].name);
        }
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
        if (isActive)
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
        if (isActive)
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

    private void EquipTile(int _tileID)
    {
        selectedTileIndex = _tileID;
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
