using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBoxController : MonoBehaviour
{
    //Non-Serialized fields
    protected bool dialogueRead = false;

    public void RegisterDialogueRead()
    {
        dialogueRead = true;
    }

    protected void ResetDialogueRead()
    {
        dialogueRead = false;
    }
}
