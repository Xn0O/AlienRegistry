using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionPanel : MonoBehaviour
{
    [Header("Animation")] public RectTransform portrait;

    public float enterX = -700;
    public float centerX;
    public float exitX = 700;

    public RectTransform arm;

    [Header("Top")] [SerializeField] private Button settingsButton;

    [Header("Scan Panel")] [SerializeField]
    private GameObject scanPanel;

    [SerializeField] private Image scanImage;
    [SerializeField] private Image xrayImage;

    [Header("Progress")] public TMP_Text progressText;

    [Header("Buttons")] [SerializeField] private Button dispatchButton;

    [SerializeField] private Button askButton;
    [SerializeField] private Button scanButton;
    [SerializeField] private Button xrayButton;

    [Header("Question List")] [SerializeField]
    private GameObject questionList;

    [SerializeField] private Button q1Button;
    [SerializeField] private Button q2Button;
    [SerializeField] private Button q3Button;


    [HideInInspector] public TMP_Text q1Text;
    [HideInInspector] public TMP_Text q2Text;
    [HideInInspector] public TMP_Text q3Text;


    private void Awake()
    {
        BindEvents();
        q1Text = q1Button.GetComponentInChildren<TMP_Text>();
        q2Text = q2Button.GetComponentInChildren<TMP_Text>();
        q3Text = q3Button.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        q1Text.text = "???";
        q2Text.text = "???";
        q3Text.text = "???";
        questionList.SetActive(false);
    }

    private void BindEvents()
    {
        settingsButton.onClick.AddListener(OnClickSettings);

        dispatchButton.onClick.AddListener(OnClickDispatch);
        askButton.onClick.AddListener(OnClickAsk);
        scanButton.onClick.AddListener(OnClickScan);
        xrayButton.onClick.AddListener(OnClickXray);

        q1Button.onClick.AddListener(() => OnClickQuestionItem(1));
        q2Button.onClick.AddListener(() => OnClickQuestionItem(2));
        q3Button.onClick.AddListener(() => OnClickQuestionItem(3));
    }

    #region Animation

    public void PlayWalk(bool isExit)
    {
        portrait.DOKill();

        var x = isExit ? exitX : centerX;
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
        seq.Append(
            portrait.DOScale(Vector3.one, 0.1f)
        );
        seq.OnComplete(() => { Debug.Log("进场动画结束"); });
    }

    public void PlayRobotWalk(bool isExit)
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

            // 走一步（硬移动）
            seq.Append(
                portrait.DOAnchorPosX(targetX, stepTime)
                    .SetEase(Ease.Linear)
            );

            // 停顿（机器人感）
            seq.AppendInterval(pauseTime);
        }

        // 最后对齐（防止浮点误差）
        seq.Append(
            portrait.DOAnchorPosX(endX, 0.05f)
        );
    }


    private readonly float armEndX = 680f;
    private readonly float armMoveTime = 1f;
    private readonly float armShakeTime = 0.25f;
    private readonly float armStartX = 1200f;

    public void ArmExtend()
    {
        arm.DOKill();

        var seq = DOTween.Sequence();

        // 确保起点（可选，看你是否每次都从收回状态开始）
        arm.anchoredPosition = new Vector2(armStartX, arm.anchoredPosition.y);

        // 伸出（机械直线）
        seq.Append(
            arm.DOAnchorPosX(armEndX, armMoveTime)
                .SetEase(Ease.Linear)
        );

        // 到位震动
        seq.AppendCallback(() =>
        {
            arm.DOShakeAnchorPos(
                armShakeTime,
                new Vector2(10f, 4f),
                20,
                0,
                fadeOut: true
            );
        });

        seq.AppendInterval(armShakeTime);
    }

    public void ArmRetract()
    {
        arm.DOKill();
        var seq = DOTween.Sequence();

        seq.AppendCallback(() => { arm.DOShakeAnchorPos(0.1f, new Vector2(8f, 3f), 15, 0, fadeOut: false); });
        seq.AppendInterval(0.1f);
        seq.Append(arm.DOAnchorPosX(armStartX, armMoveTime).SetEase(Ease.Linear));
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
    }

    private void OnClickAsk()
    {
        questionList.SetActive(!questionList.activeSelf);
    }

    private bool test;

    private void OnClickScan()
    {
        ArmExtend();
        PlayWalk(test);
        test = !test;
        Debug.Log("Scan Clicked");
    }

    private void OnClickXray()
    {
        ArmRetract();
        PlayRobotWalk(test);
        test = !test;
        Debug.Log("Xray Clicked");
    }

    private void OnClickQuestionItem(int index)
    {
        Debug.Log($"Question {index} Clicked");
    }

    #endregion
}