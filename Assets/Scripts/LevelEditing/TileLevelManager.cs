using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.IO;

public enum Tilemaps
{
    Background = 10,
    OverBackground = 20,
    Terrain = 30
}

public class TileLevelManager : MonoBehaviour
{
    public static TileLevelManager Instance;

    public List<CustomTile> tiles = new List<CustomTile>();

    public List<Tilemap> tilemaps = new List<Tilemap>();

    public Dictionary<int, Tilemap> layers = new Dictionary<int, Tilemap>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        foreach (Tilemap tilemap in tilemaps)
        {
            foreach (Tilemaps num in System.Enum.GetValues(typeof(Tilemaps)))
            {
                if (tilemap.name == num.ToString())
                {
                    if (!layers.ContainsKey((int)num))
                    {
                        layers.Add((int)num, tilemap);
                    }
                }
            }
        }
    }

    public TileLevelData SaveLevel()
    {
        TileLevelData levelData = new TileLevelData();

        foreach (var layer in layers.Keys)
        {
            levelData.layers.Add(new TileLayerData(layer));
        }

        foreach (var layerData in levelData.layers)
        {
            if (!layers.TryGetValue(layerData.layerID, out Tilemap tilemap))
            {
                break;
            }

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    TileBase temp = tilemap.GetTile(new Vector3Int(x, y, 0));
                    CustomTile tempTile = tiles.Find(t => t.tile == temp);

                    if (tempTile != null)
                    {
                        layerData.tiles.Add(tempTile.index);
                        layerData.positionsX.Add(x);
                        layerData.positionsY.Add(y);
                    }
                }
            }
        }

        return levelData;
    }

    public void LoadLevel(TileLevelData _levelData)
    {
        TileLevelData levelData = _levelData;

        foreach (var layerData in levelData.layers)
        {
            if (!layers.TryGetValue(layerData.layerID, out Tilemap tilemap))
            {
                break;
            }

            tilemap.ClearAllTiles();

            for (int i = 0; i < layerData.tiles.Count; i++)
            {
                TileBase tile = tiles.Find(t => t.index == layerData.tiles[i]).tile;

                if (tile)
                {
                    tilemap.SetTile(new Vector3Int(layerData.positionsX[i], layerData.positionsY[i], 0), tile);
                }
            }
        }
    }
}

[System.Serializable]
public class TileLevelData
{
    public List<TileLayerData> layers = new List<TileLayerData>();
}

[System.Serializable]
public class TileLayerData
{
    public int layerID;
    public List<int> tiles = new List<int>();
    public List<int> positionsX = new List<int>();
    public List<int> positionsY = new List<int>();

    public TileLayerData(int _ID)
    {
        layerID = _ID;
    }
}