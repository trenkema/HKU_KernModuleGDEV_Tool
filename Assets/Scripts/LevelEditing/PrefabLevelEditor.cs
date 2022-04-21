using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.IO;

public class PrefabLevelEditor : MonoBehaviour
{
    public static PrefabLevelEditor Instance;

    [Header("References")]
    [SerializeField] Tilemap tilemap;

    [SerializeField] LayerMask prefabsLayers;

    [SerializeField] GameObject[] itemPrefabs;

    [SerializeField] Camera cam;

    [Header("Settings")]
    [SerializeField] LevelManagerType levelManagerType;

    [SerializeField] int leftSize, rightSize, upSize, bottomSize;

    GameObject selectedPrefab;

    bool isActive = false;

    Dictionary<Vector3, PrefabPlacedData> placedPrefabsData = new Dictionary<Vector3, PrefabPlacedData>();

    List<GameObject> placedPrefabs = new List<GameObject>();

    float currentRotation = 0f;

    bool isHovering = false;

    private void OnEnable()
    {
        EventSystemNew<GameObject>.Subscribe(Event_Type.EQUIP_PREFAB, EquipPrefab);
        EventSystemNew<LevelManagerType>.Subscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);

        EventSystemNew.Subscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void OnDisable()
    {
        EventSystemNew<GameObject>.Unsubscribe(Event_Type.EQUIP_PREFAB, EquipPrefab);
        EventSystemNew<LevelManagerType>.Unsubscribe(Event_Type.ENABLE_LEVEL_EDITOR, EnableLevelEditor);

        EventSystemNew.Unsubscribe(Event_Type.GAME_STARTED, GameStarted);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void GameStarted()
    {
        isActive = false;
    }

    private void LoadPlacedPrefabs(int _prefabID, Vector3 _position, float _zRotation)
    {
        GameObject instantiatedPrefab = Instantiate(itemPrefabs[_prefabID], _position, Quaternion.Euler(0f, 0f, _zRotation));

        placedPrefabs.Add(instantiatedPrefab);

        PrefabPlacedData placedData = new PrefabPlacedData();

        placedData.prefabID = _prefabID;
        placedData.zRotation = _zRotation;

        placedPrefabsData.Add(new Vector3(_position.x, _position.y, 0f), placedData);
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

    public void OnPlacePrefab(InputAction.CallbackContext _callbackContext)
    {
        if (isActive && !isHovering)
        {
            if (_callbackContext.phase == InputActionPhase.Started)
            {
                if (!GraphicRaycasterCheck.Instance.IsHittingUI())
                {
                    Vector3Int cellPos = tilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

                    Vector3 pos = tilemap.GetCellCenterWorld(cellPos);

                    bool canPlace = CanPlace(cellPos);

                    if (canPlace)
                    {
                        if (!placedPrefabsData.ContainsKey(pos))
                        {
                            Debug.Log("Can Place");

                            GameObject instantiatedPrefab = Instantiate(selectedPrefab, pos, Quaternion.Euler(0f, 0f, currentRotation));

                            placedPrefabs.Add(instantiatedPrefab);

                            int prefabID = 0;

                            for (int i = 0; i < itemPrefabs.Length; i++)
                            {
                                if (selectedPrefab == itemPrefabs[i])
                                {
                                    prefabID = i;
                                }
                            }

                            PrefabPlacedData placedData = new PrefabPlacedData();

                            placedData.prefabID = prefabID;
                            placedData.zRotation = currentRotation;

                            placedPrefabsData.Add(pos, placedData);
                        }
                    }
                    else
                    {
                        Debug.Log("Can't Place");
                    }
                }
            }
        }
    }

    public void OnDeletePrefab(InputAction.CallbackContext _callbackContext)
    {
        if (isActive && !isHovering)
        {
            if (_callbackContext.phase == InputActionPhase.Started)
            {
                if (!GraphicRaycasterCheck.Instance.IsHittingUI())
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, prefabsLayers);

                    if (hit.collider != null)
                    {
                        Debug.Log(hit.transform.name);

                        PlaceablePrefab placeablePrefab = hit.transform.GetComponentInParent<PlaceablePrefab>();

                        if (placeablePrefab != null)
                        {
                            placedPrefabsData.Remove(placeablePrefab.position);

                            Destroy(hit.transform.gameObject);
                        }
                    }
                }
            }
        }
    }

    public void OnRotatePrefab(InputAction.CallbackContext _callbackContext)
    {
        if (isActive)
        {
            if (_callbackContext.phase == InputActionPhase.Started)
            {
                currentRotation -= 90f;

                EventSystemNew<float>.RaiseEvent(Event_Type.ROTATE_ITEM, currentRotation);
            }
        }
    }

    private void EquipPrefab(GameObject _prefab)
    {
        foreach (var prefab in itemPrefabs)
        {
            if (prefab == _prefab)
            {
                selectedPrefab = prefab;

                currentRotation = 0f;
            }    
        }
    }

    private void EquipTile()
    {

    }

    private bool CanPlace(Vector3Int _pos)
    {
        Vector3Int tempPos = _pos;

        // Check Initial Position
        if (tilemap.GetTile<TileBase>(tempPos) != null)
            return false;
        // Check Left
        tempPos.x -= leftSize;
        if (tilemap.GetTile<TileBase>(tempPos) != null)
            return false;
        // Check Right
        tempPos.x += leftSize + rightSize;
        if (tilemap.GetTile<TileBase>(tempPos) != null)
            return false;
        // Check Up
        tempPos = _pos;
        tempPos.y += upSize;
        if (tilemap.GetTile<TileBase>(tempPos) != null)
            return false;
        // Check Down
        tempPos.y -= upSize + bottomSize;
        if (tilemap.GetTile<TileBase>(tempPos) != null)
            return false;

        return true;
    }

    // Save & Load
    public PrefabLevelData SaveLevel()
    {
        PrefabLevelData levelData = new PrefabLevelData();

        foreach (var placedPrefab in placedPrefabsData)
        {
            levelData.prefabIDs.Add(placedPrefab.Value.prefabID);
            levelData.positionsX.Add(placedPrefab.Key.x);
            levelData.positionsY.Add(placedPrefab.Key.y);
            levelData.zRotation.Add(placedPrefab.Value.zRotation);
        }

        return levelData;

        //string json = JsonUtility.ToJson(levelData, true);

        //File.WriteAllText(Application.dataPath + "/prefabLevel.json", json);

        //Debug.Log("Prefabs Saved");
    }

    //private void SaveLevel()
    //{
    //    PrefabLevelData levelData = new PrefabLevelData();

    //    foreach (var placedPrefab in placedPrefabsData)
    //    {
    //        levelData.prefabIDs.Add(placedPrefab.Value.prefabID);
    //        levelData.positionsX.Add(placedPrefab.Key.x);
    //        levelData.positionsY.Add(placedPrefab.Key.y);
    //        levelData.zRotation.Add(placedPrefab.Value.zRotation);
    //    }

    //    string json = JsonUtility.ToJson(levelData, true);

    //    File.WriteAllText(Application.dataPath + "/prefabLevel.json", json);

    //    Debug.Log("Prefabs Saved");
    //}

    public void LoadLevel(PrefabLevelData _levelData)
    {
        foreach (var placedPrefab in placedPrefabs)
        {
            Destroy(placedPrefab);
        }

        placedPrefabs.Clear();

        placedPrefabsData.Clear();

        PrefabLevelData levelData = _levelData;

        for (int i = 0; i < levelData.prefabIDs.Count; i++)
        {
            LoadPlacedPrefabs(levelData.prefabIDs[i], new Vector3(levelData.positionsX[i], levelData.positionsY[i], 0f), levelData.zRotation[i]);
        }

        Debug.Log("Prefabs Loaded");
    }
}

[System.Serializable]
public class PrefabLevelData
{
    public List<int> prefabIDs = new List<int>();

    public List<float> positionsX = new List<float>();
    public List<float> positionsY = new List<float>();

    public List<float> zRotation = new List<float>();
}

[System.Serializable]
public class PrefabPlacedData
{
    public int prefabID;
    public float zRotation;
}