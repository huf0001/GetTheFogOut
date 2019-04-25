using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locatable : MonoBehaviour
{
    [SerializeField] protected TileData location;

    public TileData Location { get => location; set => location = value; }
}
