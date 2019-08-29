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
    const string NORMAL = "<sprite=\"all_icons\" index=1 tint=0 color=#009900> ";
    const string WARNING = "<sprite=\"all_icons\" index=1 tint=0 color=#f0b040> ";
    const string DANGER = "<sprite=\"all_icons\" index=1 tint=0 color=#c80000> ";
    TextMeshProUGUI powerStatus;
    TextMeshProUGUI buildingStatus;
    List<GameObject> existingMessages = new List<GameObject>();

    float pChangeValue = 0;

    public string Normal { get => NORMAL; }
    public string Warning { get => WARNING; }
    public string Danger { get => DANGER; }

    [SerializeField] GameObject warningBox;
    [SerializeField] GameObject popupMessage;

    private AudioSource audioSource;
    [SerializeField] AudioClip audioDamageAlert;
    [SerializeField] AudioClip audioPowerGridOverloadedAlert;


    // Start is called before the first frame update
    void Start()
    {
        tint = GetComponent<Image>();
        resourceController = ResourceController.Instance;
        texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        audioSource = GetComponent<AudioSource>();
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
                tint.color = new Color32(0, 153, 0, 255);
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
        CheckBuildings();
    }

    private void CheckBuildings()
    {
        List<Building> buildings = resourceController.Buildings;
        WarningLevel level = WarningLevel.Normal;

        foreach (Building b in buildings)
        {
            if (b.TakingDamage)
            {
                level = WarningLevel.Danger;
                break;
            }
            else if (b.Health < b.MaxHealth)
            {
                level = WarningLevel.Warning;
            }
        }

        switch (level)
        {
            case WarningLevel.Danger:
                warnings["buildings"] = WarningLevel.Danger;
                buildingStatus.text = DANGER + "Buildings are taking damage!";
                break;
            case WarningLevel.Warning:
                warnings["buildings"] = WarningLevel.Warning;
                buildingStatus.text = WARNING + "Some buildings are damaged";
                break;
            case WarningLevel.Normal:
                warnings["buildings"] = WarningLevel.Normal;
                buildingStatus.text = NORMAL + "All buildings are healthy";
                break;
        }
    }

    private void CheckPower()
    {
        float pChange = resourceController.PowerChange;

        if ((pChange > 0 && pChangeValue > 0) || (pChange < 0 && pChangeValue < 0) || (pChange == 0 && pChangeValue == 0))
        {
            pChangeValue = pChange;
            return;
        }

        pChangeValue = pChange;

        if (pChangeValue > 0)
        {
            warnings["power"] = WarningLevel.Normal;
            powerStatus.text = NORMAL + "Power grid is stable";
            ShowMessage(WarningLevel.Normal, powerStatus.text);
            ObjectiveController.Instance.PowerOverloaded = false;
        }
        else if (pChangeValue < 0)
        {
            warnings["power"] = WarningLevel.Danger;
            powerStatus.text = DANGER + "Power consumption exceeds generation!";
            audioSource.PlayOneShot(audioPowerGridOverloadedAlert);
            ShowMessage(WarningLevel.Danger, powerStatus.text.Insert(50, "<size=70%>"));
            ObjectiveController.Instance.PowerOverloaded = true;
        }
        else
        {
            warnings["power"] = WarningLevel.Warning;
            powerStatus.text = WARNING + "Power grid is at max capacity";
            ShowMessage(WarningLevel.Warning, powerStatus.text);
            ObjectiveController.Instance.PowerOverloaded = false;
        }
    }

    /// <summary>
    /// Creates an alert pop-up that tweens in, hangs around then tweens out
    /// </summary>
    /// <param name="level">Warning level</param>
    /// <param name="txt">The text to display on the alert</param>
    /// <param name="building">The building to move the camera to when clicked on</param>
    public void ShowMessage(WarningLevel level, string txt, Building building = null)
    {
        GameObject message = Instantiate(popupMessage, GameObject.Find("Warnings").transform);
        RectTransform messageTrans = message.GetComponent<RectTransform>();
        WarningPopup warningPopup = message.GetComponent<WarningPopup>();

        if (existingMessages.Count != 0)
        {
            message.transform.position = existingMessages[existingMessages.Count - 1].transform.position + new Vector3(335, 0);
        }
        //message.transform.position -= new Vector3(0, Screen.height / 20);
        messageTrans.anchoredPosition -= new Vector2(0, Screen.height / 40);
        
        switch (level)
        {
            case WarningLevel.Normal:
                message.GetComponent<Image>().color = new Color32(0, 153, 0, 237);
                break;
            case WarningLevel.Warning:
                message.GetComponent<Image>().color = new Color32(240, 176, 64, 237);
                break;
            case WarningLevel.Danger:
                message.GetComponent<Image>().color = new Color32(200, 0, 0, 237);
                break;
        }

        message.GetComponentInChildren<TextMeshProUGUI>().text = txt;
        warningPopup.Building = building;
        existingMessages.Add(message);
        warningPopup.Index = existingMessages.IndexOf(message);

        Sequence showMessage = DOTween.Sequence();
        showMessage.Append(messageTrans.DOAnchorPosX(-330, 0.5f).SetEase(Ease.OutExpo));
        //if (building != null)
        //{
        //    showMessage.Append(messageTrans.DOShakeAnchorPos(3.5f, 5, 100));
        //}
        //else
        //{
            showMessage.AppendInterval(3.5f);
        //}
        showMessage.Append(messageTrans.DOAnchorPosX(5, 0.5f).SetEase(Ease.InExpo)).OnComplete(
        delegate
        {
            int index = existingMessages.IndexOf(message);
            existingMessages.Remove(message);
            Destroy(message);
            for (int i = index; i < existingMessages.Count; i++)
            {
                //existingMessages[i].transform.position += new Vector3(0, Screen.height / 20);
                existingMessages[i].GetComponent<WarningPopup>().Index = i;
            }
        });
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
