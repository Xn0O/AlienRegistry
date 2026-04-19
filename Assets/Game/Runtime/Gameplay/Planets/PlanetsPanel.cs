using System;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Core;
using Game.Runtime.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlanetsPanel : UIPanel
{
    [Header("卡片组的父对象")]
    public Transform cardsRoot;

    [Header("卡片列表手动匹配")]
    public List<PlanetsCard> cards = new List<PlanetsCard>();

    [Header("卡片列表自动匹配")]
    public bool autoFindCardsFromRoot = true;


    [Header("顺序显示")]
    public bool sortByPlanetId = true;
    //调用时没有传入正确星球id就会是顺序显示，从startIndex开始
    public int startIndex = 0;

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("动画设置")]
    public bool useOpenCloseAnimation = true;
    public Animator panelAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public float closeAnimDuration = 0.2f;


    private readonly List<PlanetData> planets = new List<PlanetData>();
    private readonly List<PlanetData> displayPlanets = new List<PlanetData>();
    private bool initialized;
    private OpenData currentOpenData;

    private bool isClosing;
    private Coroutine closeCoroutine;

    // 同一planetsViewKey：固定的3个planetId，一个正确的planet，两个错误的planet
    private static readonly Dictionary<string, List<int>> planetsViewCache = new Dictionary<string, List<int>>();

    public class OpenData
    {
        public int correctPlanetId = -1; // >0 时使用 1正确+2随机 的组合，不然就按顺序显示
        public string planetsViewKey;    // 同一个key会固定结果
    }

    public static void ClearPlanetsViewCache()
    {
        planetsViewCache.Clear();
    }

    public static void ClearPlanetsViewCache(string planetsViewKey)
    {
        if (string.IsNullOrWhiteSpace(planetsViewKey)) return;
        planetsViewCache.Remove(planetsViewKey);
    }

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
            cards[i].Clicked += OnCardClicked;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickClose);
    }

    public override void OnOpen(object data = null)
    {
        isClosing = false;

        currentOpenData = data as OpenData;
        LoadPlanets();

        if (currentOpenData != null && currentOpenData.correctPlanetId > 0)
        {
            BuildFixedPlanetsView(currentOpenData.correctPlanetId, currentOpenData.planetsViewKey);
        }
        else
        {
            BuildSequentialDisplay();
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

        if (sortByPlanetId)
            planets.Sort((a, b) => a.id.CompareTo(b.id));

        if (startIndex < 0) startIndex = 0;
        if (startIndex >= planets.Count) startIndex = 0;
    }

    // 顺序显示
    private void BuildSequentialDisplay()
    {
        displayPlanets.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex >= 0 && dataIndex < planets.Count)
                displayPlanets.Add(planets[dataIndex]);
        }
    }

    // 随机显示，1正确2随机
    private void BuildFixedPlanetsView(int correctPlanetId, string planetsViewKey)
    {
        displayPlanets.Clear();

        PlanetData correct = FindPlanetById(correctPlanetId);
        if (correct == null)
        {
            Debug.LogWarning($"PlanetsPanel: correctPlanetId 无效 -> {correctPlanetId}，顺序显示。");
            BuildSequentialDisplay();
            return;
        }

        string key = string.IsNullOrWhiteSpace(planetsViewKey)
            ? $"correct_{correctPlanetId}"
            : planetsViewKey;

        // 先尝试读取记录
        if (planetsViewCache.TryGetValue(key, out var cachedIds) && IsValidCachedIds(cachedIds, correctPlanetId))
        {
            FillDisplayByIds(cachedIds);
            return;
        }

        // 首次生成把顺序记录下来
        var wrongPool = new List<PlanetData>();
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i].id != correctPlanetId)
                wrongPool.Add(planets[i]);
        }

        if (wrongPool.Count < 2)
        {
            Debug.LogWarning("PlanetsPanel: 星球数据不足3个。");
            BuildSequentialDisplay();
            return;
        }

        Shuffle(wrongPool);

        var resultIds = new List<int>
        {
            correctPlanetId,
            wrongPool[0].id,
            wrongPool[1].id
        };

        Shuffle(resultIds); // 顺序也固定下来（保存的是打乱后的顺序）
        planetsViewCache[key] = resultIds;

        FillDisplayByIds(resultIds);
    }

    private bool IsValidCachedIds(List<int> ids, int correctPlanetId)
    {
        if (ids == null || ids.Count != 3) return false;

        bool hasCorrect = false;
        var set = new HashSet<int>();
        for (int i = 0; i < ids.Count; i++)
        {
            if (!set.Add(ids[i])) return false;
            if (FindPlanetById(ids[i]) == null) return false;
            if (ids[i] == correctPlanetId) hasCorrect = true;
        }

        return hasCorrect;
    }

    private void FillDisplayByIds(List<int> ids)
    {
        displayPlanets.Clear();
        for (int i = 0; i < ids.Count; i++)
        {
            var p = FindPlanetById(ids[i]);
            if (p != null) displayPlanets.Add(p);
        }
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
        if (data == null) return;

        bool isQuizMode = currentOpenData != null && currentOpenData.correctPlanetId > 0;

        if (isQuizMode)
        {
            bool isCorrect = data.id == currentOpenData.correctPlanetId;

            if (isCorrect)
            {
                Debug.Log($"[选择模式] 点击星球: {data.name} (id={data.id})，结果: 正确");
                TriggerCorrect(); // 点击正确时触发
            }
            else
            {
                Debug.Log($"[选择模式] 点击星球: {data.name} (id={data.id})，结果: 错误");
                TriggerWrong();   // 点击错误时触发
            }
        }
        else
        {
            //调用时没有传入正确星球id就会是顺序显示
            Debug.Log($"[顺序模式] 点击星球: {data.name} (id={data.id})");
            TriggerViewMode();   // 点击错误时触发
            
        }
    }

    #region 判定处理
    //传入外界参数进行相关判定处理
    private void TriggerCorrect()
    {
        // TODO: 正确逻辑
    }
    private void TriggerWrong()
    {
        // TODO: 错误逻辑
    }
    private void TriggerViewMode()
    {
        // TODO: 顺序逻辑
        //什么也不做
    }
    #endregion
    private void OnClickClose()
    {
        if (isClosing) return;

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

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
