using System;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Core;
using Game.Runtime.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlanetsPanel : UIPanel
{
    [Header("卡片组的父对象")] public Transform cardsRoot;

    [Header("卡片列表手动匹配")] public List<PlanetsCard> cards = new List<PlanetsCard>();

    [Header("卡片列表自动匹配")] public bool autoFindCardsFromRoot = true;

    [Header("关闭按钮")] public Button closeButton;

    [Header("动画设置")] public bool useOpenCloseAnimation = true;
    public Animator panelAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public float closeAnimDuration = 0.2f;

    [Header("音效设置")] public bool usePanelSfx = true;
    public string closeButtonClickSfx = "quit";
    public string judgeLockSfx = "click_dispatch";

    private readonly List<PlanetData> planets = new List<PlanetData>();
    private readonly List<PlanetData> displayPlanets = new List<PlanetData>();

    private bool initialized;
    private bool isClosing;
    private Coroutine closeCoroutine;

    private OpenData currentOpenData;
    private int resolvedCorrectPlanetId = -1;

    // 每次打开面板只允许一次判定
    private bool hasJudged;

    public class OpenData
    {
        public int characterId = -1; // 只传角色ID
    }

    #region 对外判定事件与接口

    // 对外广播判定结果
    // 参数a=是否正确，参数b=点击的星球ID
    public static event Action<bool, int> OnPlanetJudgeResult;

    // 外界订阅接口
    public static void AddJudgeResultListener(Action<bool, int> listener)
    {
        OnPlanetJudgeResult += listener;
    }

    // 外界取消订阅接口
    public static void RemoveJudgeResultListener(Action<bool, int> listener)
    {
        OnPlanetJudgeResult -= listener;
    }

    #endregion

    public override void OnInit()
    {
        if (initialized) return;
        initialized = true;

        if (autoFindCardsFromRoot && cardsRoot != null && cards.Count == 0)
        {
            cards.AddRange(cardsRoot.GetComponentsInChildren<PlanetsCard>(true));
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            // PlanetsCard.Clicked 来自 OnPointerClick（鼠标松开后才触发）
            cards[i].Clicked += OnCardClicked;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickClose);
    }

    public override void OnOpen(object data = null)
    {
        isClosing = false;
        resolvedCorrectPlanetId = -1;

        // 每次调出面板重置判定流程
        hasJudged = false;
        ResetCardsJudgeState();

        currentOpenData = data as OpenData;
        LoadPlanets();

        if (currentOpenData == null || currentOpenData.characterId <= 0)
        {
            Debug.LogWarning("PlanetsPanel: 必须传入 characterId。");
            displayPlanets.Clear();
            RefreshCards();
            return;
        }

        if (!BuildDisplayFromCharacter(currentOpenData.characterId))
        {
            displayPlanets.Clear();
        }

        RefreshCards();

        if (useOpenCloseAnimation && panelAnimator != null)
        {
            panelAnimator.ResetTrigger(closeTrigger);
            panelAnimator.SetTrigger(openTrigger);
        }
    }

    public override void OnClose()
    {
    }

    private void OnDestroy()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            cards[i].Clicked -= OnCardClicked;
        }

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnClickClose);

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);
    }

    #region 星球显示

    // 读取数据
    private void LoadPlanets()
    {
        planets.Clear();

        var dict = DataLoader.Instance?.gameData?.planets;
        if (dict == null) return;

        foreach (var kv in dict)
            planets.Add(kv.Value);
    }

    // 通过角色数据创建列表
    private bool BuildDisplayFromCharacter(int characterId)
    {
        var charDict = DataLoader.Instance?.gameData?.characters;
        if (charDict == null)
        {
            Debug.LogWarning("PlanetsPanel: characters 数据为空。");
            return false;
        }

        if (!charDict.TryGetValue(characterId, out var character) || character == null)
        {
            Debug.LogWarning($"PlanetsPanel: 找不到 characterId={characterId}。");
            return false;
        }

        if (!int.TryParse(Convert.ToString(character.homePlanet), out resolvedCorrectPlanetId) ||
            resolvedCorrectPlanetId <= 0)
        {
            Debug.LogWarning($"PlanetsPanel: characterId={characterId} 的 homePlanet 无效。");
            return false;
        }

        if (character.planetOption == null || character.planetOption.Count == 0)
        {
            Debug.LogWarning($"PlanetsPanel: characterId={characterId} 的 planetOption 为空。");
            return false;
        }

        return BuildDisplayFromPlanetIds(character.planetOption);
    }

    // 按planetOption顺序显示
    private bool BuildDisplayFromPlanetIds(IList<int> orderedIds)
    {
        displayPlanets.Clear();

        int maxCount = cards != null && cards.Count > 0 ? cards.Count : 3;
        var used = new HashSet<int>();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            int id = orderedIds[i];
            if (!used.Add(id)) continue;

            var p = FindPlanetById(id);
            if (p == null)
            {
                Debug.LogWarning($"PlanetsPanel: planetOption 中的星球ID不存在{id}");
                continue;
            }

            displayPlanets.Add(p);
            if (displayPlanets.Count >= maxCount) break;
        }

        return displayPlanets.Count > 0;
    }

    private void RefreshCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            if (i < displayPlanets.Count)
            {
                card.gameObject.SetActive(true);
                card.Bind(displayPlanets[i]);
                // 每次开面板重新开始，重置判定
                card.SetInteractionEnabled(true);
                card.SetLockedSelected(false);
            }
            else
            {
                card.Bind(null);
                card.gameObject.SetActive(false);
            }
        }
    }

    private PlanetData FindPlanetById(int id)
    {
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i].id == id) return planets[i];
        }

        return null;
    }

    #endregion

    #region 点击事件

    private void OnCardClicked(PlanetsCard card, PlanetData data)
    {
        // 面板只允许判定一次
        if (hasJudged) return;

        if (data == null) return;
        if (resolvedCorrectPlanetId <= 0)
        {
            Debug.LogWarning("PlanetsPanel: 当前没有可用的正确星球ID。");
            return;
        }

        bool isCorrect = data.id == resolvedCorrectPlanetId;

        if (isCorrect)
        {
            Debug.Log($"点击星球: {data.name} (id={data.id})，结果: 正确");
            // TriggerCorrect(); // 由外界处理
        }
        else
        {
            Debug.Log($"点击星球: {data.name} (id={data.id})，结果: 错误");
            // TriggerWrong();   // 由外界处理
        }

        // 点击后（锁定）播放音效
        PlayPanelSfx(judgeLockSfx);

        // 判定后锁定卡片
        hasJudged = true;
        LockCardsAfterJudge(card);

        // 向外广播判定结果，外界据此处理计分或关卡流程，会传递点击是否正确和星球id
        OnPlanetJudgeResult?.Invoke(isCorrect, data.id);

        // 自动关闭：不播放“关闭按钮音效”
        ClosePanel(false);
    }

    // 重置判定状态
    private void ResetCardsJudgeState()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            card.SetInteractionEnabled(true);
            card.SetLockedSelected(false);
        }
    }

    // 固定判定状态
    private void LockCardsAfterJudge(PlanetsCard selectedCard)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            if (c == null) continue;

            if (c == selectedCard)
            {
                c.SetLockedSelected(true); // 仅已点击卡片保持高亮
                c.SetInteractionEnabled(false); // 禁止交互避免状态变化
            }
            else
            {
                c.SetLockedSelected(false); // 其它卡片不显示 selectPic
                c.SetInteractionEnabled(false); // 禁止移入显示
            }
        }
    }

    #region 判定处理

    // 传入外界参数进行相关判定处理，可由外界处理
    // private void TriggerCorrect()
    // {
    //     // TODO: 正确逻辑
    // }

    // private void TriggerWrong()
    // {
    //     // TODO: 错误逻辑
    // }

    #endregion

    private void OnClickClose()
    {
        // 手动点击关闭按钮：播放关闭按钮音效
        ClosePanel(true);
    }

    private void ClosePanel(bool playCloseButtonSfx)
    {
        if (isClosing) return;

        if (playCloseButtonSfx)
            PlayPanelSfx(closeButtonClickSfx);

        // 不使用动画：立即关闭
        if (!useOpenCloseAnimation || panelAnimator == null)
        {
            UIManager.Instance.Close<PlanetsPanel>();
            return;
        }

        // 使用动画：先播动画，再关闭
        isClosing = true;
        panelAnimator.ResetTrigger(openTrigger);
        panelAnimator.SetTrigger(closeTrigger);

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeAnimDuration);
        UIManager.Instance.Close<PlanetsPanel>();
    }

    #endregion

    #region 音效方法

    // 统一音效播放入口，避免空引用/空名字
    private void PlayPanelSfx(string sfxName)
    {
        if (!usePanelSfx) return;
        if (string.IsNullOrWhiteSpace(sfxName)) return;
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySfx(sfxName);
    }

    #endregion
}
