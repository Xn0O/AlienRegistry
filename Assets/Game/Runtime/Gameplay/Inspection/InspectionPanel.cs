using System.Threading.Tasks;
using DG.Tweening;
using Game.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionPanel : MonoBehaviour
{
    [Header("Animation")] public RectTransform portrait; //窗口中的角色

    public float enterX = -700;
    public float centerX;
    public float exitX = 700;

    public RectTransform arm;

    [Header("Top")] [SerializeField] private Button settingsButton;

    [Header("Scan Panel")] [SerializeField]
    private GameObject scanPanel;

    [SerializeField] private GlitterView glitterView;

    [Header("Character Images")] [SerializeField]
    private Image fullBodyImage; // 角色全身像(扫描窗口)

    [SerializeField] private Image xrayImage;

    [Header("Progress")] public TMP_Text progressText;

    [Header("Buttons")] [SerializeField] private Button dispatchButton;

    [SerializeField] private Button askButton;
    [SerializeField] private Button scanButton;
    [SerializeField] private Button xrayButton;

    [Header("Question List")] [SerializeField]
    private GameObject questionList;

    [SerializeField] private GameObject questionBubble;
    [SerializeField] private TMP_Text questionBubbleText;

    [SerializeField] private Button q1Button;
    [SerializeField] private Button q2Button;
    [SerializeField] private Button q3Button;


    [HideInInspector] public TMP_Text q1Text;
    [HideInInspector] public TMP_Text q2Text;
    [HideInInspector] public TMP_Text q3Text;

    [HideInInspector] public Image portraitImage;

    private GameObject currentGlitterEffect;
    private CharacterData currentData;

    private void Awake()
    {
        BindEvents();
        q1Text = q1Button.GetComponentInChildren<TMP_Text>();
        q2Text = q2Button.GetComponentInChildren<TMP_Text>();
        q3Text = q3Button.GetComponentInChildren<TMP_Text>();
        portraitImage = portrait.GetComponentInChildren<Image>();
    }

    private void Start()
    {
        ResetQuestionTexts();
    }

    private void BindEvents()
    {
        settingsButton.onClick.AddListener(OnClickSettings);

        dispatchButton.onClick.AddListener(OnClickDispatch);
        askButton.onClick.AddListener(OnClickAsk);
        scanButton.onClick.AddListener(OnClickScan);
        xrayButton.onClick.AddListener(OnClickXray);

        q1Button.onClick.AddListener(() => OnClickQuestionItem(0));
        q2Button.onClick.AddListener(() => OnClickQuestionItem(1));
        q3Button.onClick.AddListener(() => OnClickQuestionItem(2));
    }

    #region ScanScreen

    public void UpdateScreenImage(Sprite pSprite, Sprite fSprite, Sprite xSprite, CharacterData data)
    {
        currentData = data;
        portraitImage.sprite = pSprite;
        fullBodyImage.sprite = fSprite;
        xrayImage.sprite = xSprite;
        portraitImage.SetNativeSize();
        fullBodyImage.SetNativeSize();
        xrayImage.SetNativeSize();

        //初始状态：只显示全身图，隐藏X光和闪光点
        fullBodyImage.gameObject.SetActive(true);
        xrayImage.gameObject.SetActive(false);

        if (currentGlitterEffect != null) Destroy(currentGlitterEffect);
        var prefab = Resources.Load<GameObject>($"ScanPrefabs/{data.glitterPrefab}");
        if (prefab != null)
        {
            currentGlitterEffect = Instantiate(prefab, fullBodyImage.transform);
            currentGlitterEffect.SetActive(false);
        }
    }

    #endregion


    #region Button Callbacks

    private void OnClickSettings()
    {
        UIManager.Instance.Open<SettingsPanel>(true);
    }

    private void OnClickDispatch()
    {
        Debug.Log("Dispatch Clicked/ Open PlanetPanel");
        UIManager.Instance.Open<PlanetsPanel>(new PlanetsPanel.OpenData() { characterId = currentData.id });
    }

    private bool showQList;

    private void OnClickAsk()
    {
        showQList = !showQList;
        // questionList.transform.localScale = showQList ? Vector3.one : Vector3.zero;
    }

    private void OnClickScan()
    {
        Debug.Log("Scan Clicked");
        fullBodyImage.gameObject.SetActive(true);
        xrayImage.gameObject.SetActive(false);
        if (currentGlitterEffect != null) currentGlitterEffect.SetActive(true);
    }

    private void OnClickXray()
    {
        Debug.Log("Xray Clicked");
        fullBodyImage.gameObject.SetActive(false);
        xrayImage.gameObject.SetActive(true);
        InspectionManager.Instance.RegisterXrayView();
    }

    private void OnClickQuestionItem(int index)
    {
        if ((index == 0 && InspectionManager.Instance.hasViewedGlitters) ||
            (index == 1 && InspectionManager.Instance.hasViewedXray) ||
            (index == 2 && InspectionManager.Instance.hasViewitems))

        {
            questionBubble.SetActive(true);
            questionBubbleText.text = currentData.questions[index];
        }
    }

    #endregion

    #region Question

    public void ResetQuestionTexts()
    {
        q1Text.text = "???";
        q2Text.text = "???";
        q3Text.text = "???";
        questionBubble.SetActive(false);
        // questionList.transform.localScale = Vector3.zero; //TODO:改为显示小横条
    }

    public void SetQuestionText(int index, string content)
    {
        switch (index)
        {
            case 0: q1Text.text = content; break;
            case 1: q2Text.text = content; break;
            case 2: q3Text.text = content; break;
        }
        // 可在这里播一个解锁的音效或特效
        // 需要更新小红点
    }

    #endregion

    #region Animation

    public async Task PlayWalkAsync(bool isExit)
    {
        portrait.DOKill();


        var x = isExit ? exitX : centerX;
        var startX = isExit ? centerX : enterX;
        portrait.anchoredPosition = new Vector2(startX, portrait.anchoredPosition.y);
        var seq = DOTween.Sequence();

        var stepDuration = 0.5f;
        var steps = 4; // 步数（可以调）

        seq.Join(
            portrait.DOAnchorPosX(x, stepDuration * steps)
                .SetEase(Ease.Linear)
        );
        seq.Join(
            portrait.DOAnchorPosY(-100f, stepDuration)
                .SetLoops(steps, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );

        // 步伐（压缩+拉伸）
        seq.Join(
            portrait.DOScaleX(1.08f, stepDuration)
                .SetLoops(steps, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );
        seq.Join(
            portrait.DOScaleY(0.92f, stepDuration)
                .SetLoops(steps, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );

        //最后恢复正常
        seq.Append(portrait.DOScale(Vector3.one, 0.1f));
        seq.SetLink(portrait.gameObject);

        await seq.Play().AsyncWaitForCompletion();
        Debug.Log(isExit ? "离场动画结束" : "进场动画结束");
    }

    public async Task PlayRobotWalk(bool isExit)
    {
        var x = isExit ? exitX : centerX;
        portrait.DOKill();
        var steps = 7; // 从 -700 到 0，大约 7 步
        var startX = isExit ? centerX : enterX;
        var endX = isExit ? exitX : centerX;

        var stepTime = 0.25f;
        var pauseTime = 0.1f;

        var stepDistance = (endX - startX) / steps;

        // 强制起点
        portrait.anchoredPosition = new Vector2(startX, portrait.anchoredPosition.y);

        var seq = DOTween.Sequence();

        for (var i = 1; i <= steps; i++)
        {
            var targetX = startX + stepDistance * i;

            // 走一步停一下
            seq.Append(portrait.DOAnchorPosX(targetX, stepTime).SetEase(Ease.Linear));
            seq.AppendInterval(pauseTime);
        }

        // 最后对齐（防止浮点误差）
        seq.Append(portrait.DOAnchorPosX(endX, 0.05f));
        seq.SetLink(portrait.gameObject);

        await seq.Play().AsyncWaitForCompletion();
        Debug.Log(isExit ? "机器人离场动画结束" : "机器人进场动画结束");
    }


    private readonly float armEndX = 710f;
    private readonly float armMoveTime = 1f;
    private readonly float armShakeTime = 0.25f;
    private readonly float armStartX = 1200f;

    public async Task ArmExtendAsync()
    {
        arm.DOKill();

        var seq = DOTween.Sequence();

        // 确保起点（可选，看你是否每次都从收回状态开始）
        arm.anchoredPosition = new Vector2(armStartX, arm.anchoredPosition.y);

        // 伸出（机械直线）
        seq.Append(arm.DOAnchorPosX(armEndX, armMoveTime).SetEase(Ease.Linear));

        // 到位震动
        seq.Append(
            arm.DOShakeAnchorPos(
                armShakeTime,
                new Vector2(10f, 4f),
                20,
                0,
                fadeOut: true
            ));

        seq.AppendInterval(armShakeTime);
        seq.SetLink(arm.gameObject);
        await seq.Play().AsyncWaitForCompletion();
    }

    public async Task ArmRetractAsync()
    {
        arm.DOKill();
        var seq = DOTween.Sequence();

        seq.Append(arm.DOShakeAnchorPos(0.1f, new Vector2(8f, 3f), 15, 0, fadeOut: false));
        seq.AppendInterval(0.1f);
        seq.Append(arm.DOAnchorPosX(armStartX, armMoveTime).SetEase(Ease.Linear));
        seq.SetLink(arm.gameObject);
        await seq.Play().AsyncWaitForCompletion();
    }

    #endregion
}