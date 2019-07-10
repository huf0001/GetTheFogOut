using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locatable : MonoBehaviour
{
    [Header("Locatable Location")]
    [SerializeField] protected TileData location = null;

    public virtual TileData Location { get => location; set => location = value; }
}
