using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningScript : MonoBehaviour
{
    Image tint;
    WarningLevel state = WarningLevel.Normal;
    Dictionary<string, object[]> warnings = new Dictionary<string, object[]>
    {
        { "power", new object[] { "Power is stable", WarningLevel.Normal } }
    };
    Hub hub;
    object[] powerState;

    // Start is called before the first frame update
    void Start()
    {
        tint = GetComponent<Image>();
        hub = WorldController.Instance.Hub;
        powerState = warnings["power"];
    }

    // Update is called once per frame
    void Update()
    {
        CheckStates();

        WarningLevel level = WarningLevel.Normal;
        foreach (object[] w in warnings.Values)
        {
            if ((WarningLevel)w[1] == WarningLevel.Warning && level == WarningLevel.Normal)
            {
                level = (WarningLevel)w[1];
            }
            if ((WarningLevel)w[1] == WarningLevel.Danger && (level == WarningLevel.Normal || level == WarningLevel.Warning))
            {
                level = (WarningLevel)w[1];
                break;
            }
        }
        ChangeState(level);
    }

    public enum WarningLevel
    {
        Normal,
        Warning,
        Danger
    }
    
    private void ChangeState(WarningLevel w)
    {
        switch (w)
        {
            case WarningLevel.Normal:
                tint.color = new Color32(63, 63, 63, 255);
                state = w;
                break;
            case WarningLevel.Warning:
                tint.color = new Color32(240, 176, 64, 255);
                state = w;
                break;
            case WarningLevel.Danger:
                tint.color = new Color32(200, 0, 0, 255);
                state = w;
                break;
        }
    }

    private void CheckStates()
    {
        if (hub.PowerChange > 0)
        {
            powerState[0] = "Power is stable";
            powerState[1] = WarningLevel.Normal;
        }
        else if (hub.PowerChange < 0)
        {
            powerState[0] = "Power is overloaded!";
            powerState[1] = WarningLevel.Danger;
        }
        if (hub.PowerChange == 0)
        {
            powerState[0] = "Power is at max capacity";
            powerState[1] = WarningLevel.Warning;
        }
    }

    //public void AddWarning(string message, WarningLevel warningLevel)
    //{
    //    bool added = false;
    //    foreach (string key in warnings.Keys)
    //    {
    //        if (key == message) { added = true; break; }
    //    }
    //    if (!added) { warnings.Add(message, warningLevel); }
    //}

    //public void RemoveWarning()
    //{

    //}
}
