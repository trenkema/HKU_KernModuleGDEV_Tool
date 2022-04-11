using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablePrefab : MonoBehaviour
{
    public Vector3 position { get; private set; }

    private void Start()
    {
        position = transform.position;
    }
}
