using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.IO;

public enum LevelManagerType
{
    TileManager,
    PrefabManager
}

public class PrefabLevelManager : MonoBehaviour
{
    public static PrefabLevelManager Instance;

    public Dictionary<GameObject, GameObject> placedPrefabs = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
