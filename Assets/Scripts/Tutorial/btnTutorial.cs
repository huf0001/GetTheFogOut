using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class btnTutorial : MonoBehaviour
{
    //Non-Serialized Fields
    private TutorialController tutorialController;
    private bool reportClick = false;

    //Public Properties
    public bool ReportClick { get => reportClick; set => reportClick = value; }

    private void Start()
    {
        tutorialController = WorldController.Instance.TutorialController;
    }

    public void ReportClickToTutorialController()
    {
        if (reportClick)
        {
            Debug.Log(tutorialController);
            tutorialController.RegisterButtonClicked();
        }
    }
}
