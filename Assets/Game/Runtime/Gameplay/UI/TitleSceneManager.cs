using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    
    public Button startGameButton;
    public Button settingButton;
    public Button membersButton;
    public Button exitGameButton;

    private void Start()
    {
        // if (settingButton)
            // settingButton.onClick.AddListener(() => UIManager.Instance.Open<SettingsPanel>());
        if (startGameButton) startGameButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());
        if (exitGameButton) exitGameButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
    }
}
