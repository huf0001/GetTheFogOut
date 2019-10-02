using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnTutorial : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [SerializeField] private ButtonType buildingType;

    //Non-Serialized Fields
    private TutorialController tutorialController;
    [SerializeField] private Button button;

    //Public Properties
    public Button Button { get => button; }
    public ButtonType ButtonType { get => buildingType; }

    //Setup Methods----------------------------------------------------------------------------------------------------------------------------------

    //Gets some necessary values
    private void Start()
    {
        button = GetComponent<Button>();
        tutorialController = TutorialController.Instance;
    }

    //Recurring Methods------------------------------------------------------------------------------------------------------------------------------

    //Controls when lerping happens based on outside variables
    private void Update()
    {
        if (tutorialController.Stage == TutorialStage.Finished)
        {
            button.interactable = true;
            Destroy(this);
        }
        else if ((UIController.instance.buildingSelector.Visible || buildingType == ButtonType.Upgrades || buildingType == ButtonType.Destroy) && tutorialController.ButtonAllowed(buildingType))
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }
}
