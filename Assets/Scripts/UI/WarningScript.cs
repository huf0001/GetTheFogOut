using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WarningScript : MonoBehaviour
{
    Image tint;
    WarningLevel state = WarningLevel.Normal;
    Dictionary<string, WarningLevel> warnings = new Dictionary<string, WarningLevel>
    {
        { "power", WarningLevel.Normal },
        { "buildings", WarningLevel.Normal }
    };
    ResourceController resourceController;

    TextMeshProUGUI[] texts;
    const string NORMAL = "<sprite=\"all_icons\" index=4 tint=1 color=#3f3f3f> ";
    const string WARNING = "<sprite=\"all_icons\" index=4 tint=1 color=#f0b040> ";
    const string DANGER = "<sprite=\"all_icons\" index=4 tint=1 color=#c80000> ";
    TextMeshProUGUI powerStatus;
    TextMeshProUGUI buildingStatus;
    List<GameObject> existingMessages = new List<GameObject>();

    int pChangeValue = 0;

    [SerializeField] GameObject warningBox;
    [SerializeField] GameObject popupMessage;

    // Start is called before the first frame update
    void Start()
    {
        tint = GetComponent<Image>();
        resourceController = WorldController.Instance.ResourceController;
        texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        powerStatus = texts[0];
        powerStatus.text = NORMAL + "Power is stable";
        buildingStatus = texts[1];
        buildingStatus.text = NORMAL + "All buildings are healthy";
    }

    // Update is called once per frame
    void Update()
    {
        CheckStates();

        WarningLevel level = WarningLevel.Normal;
        foreach (WarningLevel w in warnings.Values)
        {
            if (w == WarningLevel.Warning && level == WarningLevel.Normal)
            {
                level = w;
            }
            if (w == WarningLevel.Danger && (level == WarningLevel.Normal || level == WarningLevel.Warning))
            {
                level = w;
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
        if (resourceController.PowerChange != pChangeValue)
        {
            CheckPower();
        }
    }

    private void CheckPower()
    {
        int pChange = resourceController.PowerChange;
        if ((pChange > 0 && pChangeValue > 0) || (pChange < 0 && pChangeValue < 0) || (pChange == 0 && pChangeValue == 0)) { pChangeValue = pChange; return; }
        pChangeValue = pChange;

        if (pChangeValue > 0)
        {
            warnings["power"] = WarningLevel.Normal;
            powerStatus.text = NORMAL + "Power grid is stable";
            StartCoroutine(ShowMessage(powerStatus.text));
        }
        else if (pChangeValue < 0)
        {
            warnings["power"] = WarningLevel.Danger;
            powerStatus.text = DANGER + "Power grid is overloaded!";
            StartCoroutine(ShowMessage(powerStatus.text));
        }
        else
        {
            warnings["power"] = WarningLevel.Warning;
            powerStatus.text = WARNING + "Power grid is at max capacity";
            StartCoroutine(ShowMessage(powerStatus.text));
        }
    }

    private IEnumerator ShowMessage(string txt)
    {
        GameObject message = Instantiate(popupMessage, GameObject.Find("Canvas").transform);
        if (existingMessages.Count != 0)
        {
            message.transform.position = existingMessages[existingMessages.Count - 1].transform.position + new Vector3(335, 0);
        }
        message.transform.position -= new Vector3(0, 50);
        message.GetComponentInChildren<TextMeshProUGUI>().text = txt;
        existingMessages.Add(message);
        message.GetComponent<RectTransform>().DOAnchorPosX(-330, 0.5f).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(3.5f);
        message.GetComponent<RectTransform>().DOAnchorPosX(5, 0.5f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(0.5f);
        existingMessages.Remove(message);
        Destroy(message);
    }

    public void ToggleVisibility()
    {
        warningBox.SetActive(!warningBox.activeSelf);
        foreach (GameObject go in existingMessages)
        {
            go.SetActive(!go.activeSelf);
        }
    }
}
