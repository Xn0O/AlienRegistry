using Game.Runtime.Core;
using Game.Runtime.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InspectionManager : MonoBehaviour
{
    public static InspectionManager Instance;

    [Header("Game Progress")] public int currentCharacterIndex; // 当前是第几个外星人 (0-4)

    public List<int> characterIds = new()
    {
        1,
        2,
        3,
        4,
        5
    }; // 假设这是你要检查的5个人ID

    public int correctCount; // 正确发配的数量

    [Header("UI Parts")] public InspectionPanel inspectionPanel;
    public PadPanel padPanel;
    public PackageView packageView;
    public PlanetsPanel dispatchPanel; //星球面板

    //解锁状态追踪
    private readonly HashSet<int> clickedGlitterPoints = new(); // 记录点过的闪光点索引
    private readonly HashSet<int> viewedItemIds = new(); // 记录点过的道具ID

    private CharacterData currentData;
    private bool hasViewedGlitters;
    private bool hasViewedXray; // 是否看过了X光
    private bool hasViewitems;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 游戏开始，载入第一个人
        LoadRound(0);
    }

    public void LoadRound(int index)
    {
        if (index >= characterIds.Count)
        {
            TriggerEnding();
            return;
        }

        Debug.Log($"开始游戏 Round={index}");

        currentCharacterIndex = index;
        int id = characterIds[index];

        if (DataLoader.Instance.gameData.characters.TryGetValue(id, out currentData))
        {
            // 更新UI进度显示
            inspectionPanel.progressText.text = $"{currentCharacterIndex + 1} / {characterIds.Count}";
            _ = UpdateCharacterDisplay(currentData);
        }

        // 重置状态
        clickedGlitterPoints.Clear();
        viewedItemIds.Clear();
        hasViewedXray = false;

        inspectionPanel.ResetQuestionTexts();
    }

    private async Task UpdateCharacterDisplay(CharacterData data)
    {
        // 加载图片
        Sprite avater = Resources.Load<Sprite>($"Character/{data.profile}");
        Sprite pSprite = Resources.Load<Sprite>($"Character/{data.portrait}");
        Sprite fSprite = Resources.Load<Sprite>($"Character/{data.fullBody}");
        Sprite xSprite = Resources.Load<Sprite>($"Character/{data.xray}");

        // 刷新扫描窗口
        inspectionPanel.UpdateScreenImage(pSprite, fSprite, xSprite, data.glitterPrefab);

        // 刷新 PadPanel 角色信息
        padPanel.UpdateProfileData(data.name, data.species, avater, data.description);

        // 刷新背包数据
        packageView.RefreshView(data);

        Task t1 = inspectionPanel.PlayWalkAsync(false);
        Task t2 = inspectionPanel.ArmExtendAsync();

        await Task.WhenAll(t1, t2);
        Debug.Log("入场动画播完了，玩家现在可以开始操作了");
    }

    /// <summary>
    ///     当玩家在发配面板点击了某个星球后的回调
    /// </summary>
    /// <param name="planetId">玩家选择的星球ID</param>
    public void OnDispatchCallback(int planetId)
    {
        if (DataLoader.Instance.gameData.planets.TryGetValue(planetId, out PlanetData planet))
        {
            Debug.Log($"你把 {currentData.name} 发配到了 {planet.name}");

            // 逻辑判定：是否发配回了正确星球
            if (planet.id == currentData.homePlanet)
            {
                correctCount++;
                Debug.Log("发配正确！");
            }
            else
            {
                Debug.Log("发配错误...");
            }

            // 执行离场并进入下一轮
            NextRoundSequenceAsync();
        }
    }

    private async void NextRoundSequenceAsync()
    {
        // 1. 同时开启两个动画，但不立即 await 它们
        // 这会让机械臂收回和角色走开【同时开始】
        Task armTask = inspectionPanel.ArmRetractAsync();
        Task walkTask = inspectionPanel.PlayWalkAsync(true);

        // 2. 等待两个任务全部完成
        // 即使一个播 1s，一个播 2s，代码也会等最长的那个播完
        await Task.WhenAll(armTask, walkTask);

        // 3. 两个都播完了，加载下一个人
        LoadRound(currentCharacterIndex + 1);
    }

    private void TriggerEnding()
    {
        float score = (float)correctCount / characterIds.Count;
        Debug.Log($"游戏结束！最终得分: {correctCount}. 正确率: {score * 100}%");

        if (correctCount >= 5)
        {
            // 完美结局
        }
        else if (correctCount >= 3)
        {
            // 普通结局
        }
        // 糟糕结局
        // UIManager.Instance.Open<ResultPanel>(score);
    }

    #region 条件触发方法

    public void OnGlitterClicked(int index)
    {
        if (hasViewedGlitters)
        {
            return;
        }

        clickedGlitterPoints.Add(index);
        if (clickedGlitterPoints.Count >= 3)
        {
            hasViewedGlitters = true;
            UnlockQuestion(0); // 解锁第一个问题
        }
    }

    // 2. X光查看 (由 InspectionPanel.OnClickXray 调用)
    public void RegisterXrayView()
    {
        if (!hasViewedXray)
        {
            hasViewedXray = true;
            UnlockQuestion(1); // 解锁第二个问题
        }
    }

    // 3. 道具查看 (由 PackageView 在点击 Slot 时调用)
    public void OnItemViewed(int itemId)
    {
        if (hasViewitems)
        {
            return;
        }

        viewedItemIds.Add(itemId);
        if (currentData == null)
        {
            return;
        }

        // 检查是否所有道具都看过了
        if (viewedItemIds.Count >= currentData.itemIds.Count)
        {
            hasViewitems = true;
            UnlockQuestion(2); // 解锁第三个问题
        }
    }

    private void UnlockQuestion(int index)
    {
        Debug.Log($"解锁问题 index={index}");
        if (currentData == null)
        {
            return;
        }

        // 调用 Panel 更新文字
        inspectionPanel.SetQuestionText(index, currentData.shortQuestions[index]);
    }

    #endregion
}