using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UserInterface : MonoBehaviour
{
	public Text res1Text, res2Text, res3Text, res4Text, pauseText;
	public int res1, res2, res3, res4;
	
	// private bool isPause;
    
    void Start()
    {
    	// isPause = false;
        res1 = 0;
        res2 = 0;
        res3 = 0;
        res4 = 0;
     	pauseText.text = "";
        res1Text.text = "Power: " + res1.ToString();
        res2Text.text = "Organic: " + res2.ToString();
        res3Text.text = "Mineral: " + res3.ToString();
        res4Text.text = "Fuel: " + res4.ToString();
    }

    void FixedUpdate()
    {
        res1Text.text = "Power: " + res1.ToString();
        res2Text.text = "Organic: " + res2.ToString();
        res3Text.text = "Mineral: " + res3.ToString();
        res4Text.text = "Fuel: " + res4.ToString();

        // if (Input.GetKeyDown("p"))
        // {
        // 	isPause = !isPause;
        // }
        // if (isPause)
        // {
        // 	pauseText.text = "Paused";
        // }
        // else
        // {
        // 	pauseText.text = "";
        // }

    }
}
