using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridToMouse : MonoBehaviour
{
    [SerializeField] private int radius = 5;
    private int i = 0;
    // Update is called once per frame
    void Update()
    {
        visibleGrid();
    }

    void visibleGrid()
    {
         Collider[] hitColliders = Physics.OverlapSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), radius);
        foreach(Collider hit in hitColliders)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
