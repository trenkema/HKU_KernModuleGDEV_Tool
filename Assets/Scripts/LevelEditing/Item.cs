using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public CustomTile customTile;
    public GameObject prefab;
    public GameObject dragImage;
    public Sprite itemImage;
    public Vector2 itemImageSize;
}
