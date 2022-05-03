using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablePrefab : MonoBehaviour
{
    public Vector3 position { get; private set; }

    [SerializeField] int leftSize, rightSize, upSize, bottomSize;

    public int leftSizeGet { get { return leftSize; } }
    public int rightSizeGet { get { return rightSize; } }
    public int upSizeGet { get { return upSize; } }
    public int bottomSizeGet { get { return bottomSize; } }

    private void Start()
    {
        position = transform.position;
    }
}
