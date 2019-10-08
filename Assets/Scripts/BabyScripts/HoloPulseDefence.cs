using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloPulseDefence : MonoBehaviour
{
    public GameObject rangeIndicator;
    
    // Start is called before the first frame update
    void Start()
    {
        if (WorldController.Instance.pulseDefUpgradeLevel)
        {
            switch (WorldController.Instance.pulseDefUpgradeLevel.pathNum)
            {
                case 1:
                    switch (WorldController.Instance.pulseDefUpgradeLevel.upgradeNum)
                    {
                        case 1:
                            rangeIndicator.transform.localScale = new Vector3(7f, 7f, 7f);
                            break;
                        case 2:
                            rangeIndicator.transform.localScale = new Vector3(9f, 7f, 9f);
                            break;
                    }
                    break;
            }
        }
    }
}
