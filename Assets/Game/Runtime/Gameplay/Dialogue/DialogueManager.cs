using System;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Core;
using Game.Runtime.Data;
using UnityEngine;
using EventHandler = Game.Runtime.Core.EventHandler;


public class DialogueManager : Singleton<DialogueManager>
{
    private int currentDialogueId;
    private int nextDialogueId;
    private DialoguePanel dialogueView;

    // private bool finished;
    private GamePhase lastPhase;
    private Action onFinishedAction;

    private void Start()
    {
        // 预加载一下,防卡
        if (!UIManager.Instance.IsPanelOpen<DialoguePanel>())
            dialogueView = UIManager.Instance.CreatePanel<DialoguePanel>();
    }

    /// <summary>
    ///     探索界面对话入口
    /// </summary>
    /// <param name="dialogueId">对话ID</param>
    public void PlayDialogue(int dialogueId)
    {
        if (DataLoader.Instance.gameData.dialogues.TryGetValue(dialogueId, out var dialogueData))
        {
            currentDialogueId = dialogueId;
            nextDialogueId =  dialogueData.nextDialogueId;
            EventHandler.CallDialogueStartEvent(dialogueId);
            var panel = UIManager.Instance.Open<DialoguePanel>();
            panel.ShowDialogue(dialogueData.lines);
        }
        else
        {
            Debug.LogWarning($"找不到对话 dialogueId={dialogueId}");
        }
    }

    public void ShowDialogueString(string text)
    {
        UIManager.Instance.Open<DialoguePanel>();
        var line = new DialogueLine { type = DialogueType.Null, text = text };
        var lines = new List<DialogueLine> { line };

        dialogueView.ShowDialogue(lines);
    }

    public void CancelDialogue()
    {
        UIManager.Instance.Close<DialoguePanel>();
    }

    public void FinishDialogue()
    {
        UIManager.Instance.Close<DialoguePanel>();

        onFinishedAction?.Invoke();
        onFinishedAction = null;

        // Debug.Log($"结束对话 {currentDialogueId}");
        EventHandler.CallDialogueFinishedEvent(currentDialogueId);
        if (nextDialogueId != 0)
        {
            PlayDialogue(nextDialogueId);
        }
    }
}