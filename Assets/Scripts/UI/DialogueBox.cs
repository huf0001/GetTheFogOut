using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateDialogueBox(string text)
    {
        textBox.text = text;
        gameObject.SetActive(true);
    }

    public void DeactivateDialogueBox()
    {
        gameObject.SetActive(false);
        textBox.text = "";
    }
}
