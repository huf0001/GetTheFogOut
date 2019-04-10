using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour
{

    public RectTransform rectTransform;
    public TextMeshProUGUI toolTipText;
    public Vector2 offset;
    public LayerMask layerMask;

    void Start()
    {
        offset = new Vector2(5, 5);
    }

    void Update()
    {
        RaycastHit hit = new RaycastHit();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, layerMask))
        {
            if (hit.transform.CompareTag("Building") || hit.transform.CompareTag("Resource"))
            {
                rectTransform.gameObject.SetActive(true);
                if (hit.transform.name == "Hub(Clone)")
            	{
            		toolTipText.text = "Hub, your only chance off this planet!";
            	}
            	else if (hit.transform.name == "Battery")
            	{
            		toolTipText.text = "Battery, for when you need extra energy!";
            	}
            	else if (hit.transform.name == "Defence")
            	{
            		toolTipText.text = "Defence, demistifies your surroundings!";
            	}
            	else if (hit.transform.name == "Generator")
            	{
            		toolTipText.text = "Generator, store your zaps!";
            	}
            	else if (hit.transform.name == "Harvester")
            	{
            		toolTipText.text = "Harvester, crystals are enlightening!";
            	}
            	else if (hit.transform.name == "Relay")
            	{
            		toolTipText.text = "Relay, connect your devices!";
            	}
                else
                {
                	toolTipText.text = hit.transform.name;
                }

                rectTransform.position = new Vector3(Input.mousePosition.x + offset.x , Input.mousePosition.y + offset.y , 0);
            }
            else
            {
            	rectTransform.gameObject.SetActive(false);
                toolTipText.text = null;
            }
        }
        else
        {
            rectTransform.gameObject.SetActive(false);
            toolTipText.text = null;
        }
    }
}
