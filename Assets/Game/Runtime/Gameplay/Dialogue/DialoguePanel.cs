using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialoguePanel : UIPanel
{
    [Header("UI Elements")] public Button nextButton;

    public GameObject normalDialoguePanel;
    public GameObject blackDialoguePanel;
    // public Button skipButton;
    public GameObject nextArrow;

    [Header("角色对话")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI dialogueTextLabel;

    [Header("选项")] public GameObject optionGroup;

    [Header("黑屏对话")] public TextMeshProUGUI blackTextLabel;

    [Header("打字效果显示速度")] public float textSpeed = 0.02f;

    private List<DialogueLine> dialogueLines = new();
    private int index;

    private Button[] optionButtons;
    private Coroutine typingCoroutine;
    private bool typingFinished;
    private bool waitingForOption;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextClick);
        // if (blackDialoguePanel) blackDialoguePanel.GetComponent<Button>().onClick.AddListener(OnNextClick);
        //skipButton.GetComponent<Button>().onClick.AddListener(OnSkipClick);
        //cancelButton.GetComponent<Button>().onClick.AddListener(OnCancelClick);
        optionButtons = optionGroup.GetComponentsInChildren<Button>();
    }
    
    private void Update()
    {
        if (!waitingForOption && Keyboard.current.spaceKey.wasPressedThisFrame) nextButton.onClick.Invoke();
    }

    private void OnNextClick()
    {
        if (index == dialogueLines.Count)
        {
            DialogueManager.Instance.FinishDialogue();
            return;
        }

        if (typingFinished)
        {
            ShowDialogueLine();
        }
        else
        {
            // 停止协程并直接显示完整文字
            StopCoroutine(typingCoroutine);
            typingFinished = true;
            dialogueTextLabel.text = dialogueLines[index].text;
            if (blackTextLabel) blackTextLabel.text = dialogueLines[index].text;
            nextArrow.SetActive(true);
            index++;
        }
    }

    /// <summary>
    ///     显示对话面板
    /// </summary>
    /// <param name="lines"></param>
    public void ShowDialogue(List<DialogueLine> lines)
    {
        index = 0;
        dialogueLines = lines;

        HideOptions();
        ShowDialogueLine();
    }

    private void ShowDialogueLine()
    {
        blackDialoguePanel.SetActive(false);
        var line = dialogueLines[index];
        switch (line.type)
        {
            case DialogueType.Normal:
                // 普通角色对话

                nameLabel.text = line.speakerName;
                typingCoroutine = StartCoroutine(TypingText(dialogueTextLabel));
                break;

            case DialogueType.Null:
                // 没有角色，旁白等

                nameLabel.text = "";
                typingCoroutine = StartCoroutine(TypingText(dialogueTextLabel));
                break;

            case DialogueType.Black:
                // 全屏，黑色背景对话
                blackDialoguePanel.SetActive(true);
                typingCoroutine = StartCoroutine(TypingText(blackTextLabel));
                break;

            case DialogueType.Choice:

                ShowOptions(line.choices);
                break;
        }
    }

    private Sprite LoadAvatar(string avatarName)
    {
        var sprite = Resources.Load<Sprite>($"Character/{avatarName}");

        if (sprite == null)
            Debug.LogError($"找不到角色立绘: {avatarName}");

        return sprite;
    }

    #region 打字效果

    private IEnumerator TypingText(TextMeshProUGUI textLabel = null)
    {
        if (!textLabel) textLabel = dialogueTextLabel;
        textLabel.text = "";
        nextArrow.SetActive(false);
        typingFinished = false;
        var isAddingRichTextTag = false;
        foreach (var c in dialogueLines[index].text)
            if (c == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                textLabel.text += c;
                if (c == '>') isAddingRichTextTag = false;
            }
            else
            {
                textLabel.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

        nextArrow.SetActive(true);
        typingFinished = true;
        index++;
    }

    #endregion


    #region 选项

    private void HideOptions()
    {
        optionGroup.gameObject.SetActive(false);
        if (optionButtons == null) return;

        foreach (var btn in optionButtons)
            if (btn)
                btn.gameObject.SetActive(false);
    }

    private void ShowOptions(List<Choice> choices)
    {
        if (waitingForOption) return;
        waitingForOption = true;
        
        HideOptions();
        optionGroup.gameObject.SetActive(true);
        
        Debug.Log($"显示选项 共 {choices.Count} 个选项");
        for (var i = 0; i < choices.Count && i < choices.Count; i++)
        {
            var button = optionButtons[i];
            button.gameObject.SetActive(true); // 显示选项按钮

            // 设置按钮文本
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text)
                text.text = choices[i].text;

            // 绑定点击事件（先清空旧的）
            button.onClick.RemoveAllListeners();
            var idx = i; // 避免闭包问题
            button.onClick.AddListener(() => OnOptionSelected(choices[idx]));
        }
    }

    private void OnOptionSelected(Choice choice)
    {
        // AudioManager.Instance.PlaySFX(AudioName.Click);

        HideOptions();
        waitingForOption = false;
        DialogueManager.Instance.FinishDialogue();

        Debug.Log($"选择 【{choice.text}】 nextNode={choice.nextNodeId}");
        DialogueManager.Instance.PlayDialogue(choice.nextNodeId);
    }

    #endregion

    #region 探索对话按钮事件

    private void OnCancelClick()
    {
        if (!typingFinished) StopCoroutine(typingCoroutine);

        DialogueManager.Instance.CancelDialogue();
    }

    private void OnSkipClick()
    {
        if (!typingFinished) StopCoroutine(typingCoroutine);

        DialogueManager.Instance.FinishDialogue();
    }

    #endregion
}