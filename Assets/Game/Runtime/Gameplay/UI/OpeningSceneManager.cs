using System;
using UnityEngine;
using Game.Runtime.Core;
using EventHandler = Game.Runtime.Core.EventHandler;

public class OpeningSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        DialogueManager.Instance.PlayDialogue(1);
    }

    private void OnDialogueEnd(int dialogueId)
    {
        if (dialogueId == 4)
        {
            GameManager.Instance.EnterGameScene();
        }
    }

    private void OnEnable()
    {
        EventHandler.DialogueFinishedEvent += OnDialogueEnd;
    }
    
    private void OnDisable()
    {
        EventHandler.DialogueFinishedEvent -= OnDialogueEnd;
    }
}
