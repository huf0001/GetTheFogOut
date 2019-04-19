using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int length;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public GameObject[,] InstantiateTileArray()
    //{
    //    GameObject[,] tiles = new GameObject[width, length];
    //    Tile[] unsorted = gameObject.GetComponentsInChildren<Tile>();

    //    if (unsorted.Length != width * length)
    //    {
    //        Debug.Log("Script TILES has been given the wrong dimensions of the game board.");
    //        return null;
    //    }
    //    else
    //    {
    //        foreach (Tile t in unsorted)
    //        {
    //            Vector3 pos = t.transform.position;
    //            if (pos.x < width && pos.z < length)
    //            {
    //                //t.gameObject.name = "Tile(" + (int)pos.x + ", " + (int)pos.z + ")";
    //                tiles[(int)pos.x, (int)pos.z] = t.gameObject;
    //            }
    //            else
    //            {
    //                Debug.Log("Need to adjust position of tile at " + pos + " or adjust the algorithm of script TILES");
    //            }
    //        }
    //    }

    //    return tiles;
    //}
}
