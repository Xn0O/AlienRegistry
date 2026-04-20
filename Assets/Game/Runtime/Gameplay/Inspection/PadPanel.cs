using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// 引入 TextMeshPro 命名空间

public class PadPanel : MonoBehaviour
{
    public Button showButton;
    public Button hideButton;
    public float duration = 0.5f;
    [SerializeField] private Vector2 visiblePos = new Vector2(0, -190);
    [SerializeField] private Vector2 hiddenPos = new Vector2(360, -710);

    [SerializeField] private Vector3 visibleScale = Vector3.one;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.75f, 0.75f, 1f);

    [Header("Toggles (Tebs)")] public Toggle profileToggle;

    public Toggle packageToggle;

    [Header("Views")] public GameObject profilePanel;

    public GameObject packagePanel;
    private PackageView packageView;

    [Header("Profile View Elements")] public TextMeshProUGUI nameLabel;

    public TextMeshProUGUI speciesLabel;
    public Image avatarImage;
    public TextMeshProUGUI desText;
    public Transform questionsContainer; // 对应 Questions 节点，方便后续遍历或动态生成 Q&A

    [Header("Package View Elements")] public Transform itemGroup; // 背包物品组容器

    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDesc;

    private bool isShowing; // 记录当前是否显示

    private UIPadParallax parallax;
    private RectTransform rectTransform;
    private GameObject profileToggleOn;
    private GameObject packageToggleOn;

    private void Awake()
    {
        parallax = GetComponent<UIPadParallax>();
        rectTransform = GetComponent<RectTransform>();
        profileToggleOn = profileToggle.transform.Find("ProfileToggleOn").gameObject;
        packageToggleOn = packageToggle.transform.Find("PackageToggleOn").gameObject;
        packageView = packagePanel.GetComponent<PackageView>();

        // 初始位置设为隐藏位置
        rectTransform.anchoredPosition = hiddenPos;
        rectTransform.localScale = hiddenScale;
        isShowing = false;
    }

    private void Start()
    {
        showButton.onClick.AddListener(ShowPad);
        hideButton.onClick.AddListener(HidePad);
        profileToggle.onValueChanged.AddListener(OnProfileToggleChange);
        packageToggle.onValueChanged.AddListener(OnPackageToggleChange);

        showButton.gameObject.SetActive(true);
        hideButton.gameObject.SetActive(false);

        profileToggle.isOn = true;
        profilePanel.transform.localScale = Vector3.one;
        packagePanel.transform.localScale = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (showButton != null) showButton.onClick.RemoveAllListeners();
        if (hideButton != null) hideButton.onClick.RemoveAllListeners();
        if (profileToggle != null) profileToggle.onValueChanged.RemoveAllListeners();
        if (packageToggle != null) packageToggle.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    ///     更新 Profile 数据的方法
    /// </summary>
    public void UpdateProfileData(string pName, string pSpecies, Sprite pAvatar, string pDesc)
    {
        if (nameLabel != null) nameLabel.text = "Name: " + pName;
        if (speciesLabel != null) speciesLabel.text = "Species: " + pSpecies;
        if (avatarImage != null) avatarImage.sprite = pAvatar;
        if (desText != null) desText.text = pDesc;
        packageView.OnSlotSelected(0);
    }

    private void OnProfileToggleChange(bool isOn)
    {
        if (isOn)
        {
            AudioManager.Instance.PlaySfx("switch");
        }

        profilePanel.transform.localScale = new Vector3(isOn ? 1f : 0f, 1f, 1f);
        profileToggleOn.SetActive(isOn);
    }

    private void OnPackageToggleChange(bool isOn)
    {
        if (isOn)
        {
            AudioManager.Instance.PlaySfx("switch");
        }

        packagePanel.transform.localScale = new Vector3(isOn ? 1f : 0f, 1f, 1f);
        packageToggleOn.SetActive(isOn);
        if (isOn) packageView.OnSlotSelected(0);
    }


    public void ShowPad()
    {
        if (isShowing) return;

        showButton.gameObject.SetActive(false);

        rectTransform.DOKill();
        if (parallax != null) parallax.enabled = false;

        // 同时做位置 + 缩放
        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOAnchorPos(visiblePos, duration).SetEase(Ease.OutBack));
        seq.Join(rectTransform.DOScale(visibleScale, duration).SetEase(Ease.OutBack));

        seq.OnComplete(() =>
        {
            if (parallax != null)
            {
                parallax.SetBasePosition(rectTransform.anchoredPosition);
                parallax.enabled = true;
            }

            hideButton.gameObject.SetActive(true);
        });

        isShowing = true;
    }

    public void HidePad()
    {
        if (!isShowing) return;

        hideButton.gameObject.SetActive(false);

        rectTransform.DOKill();
        if (parallax != null) parallax.enabled = false;

        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOAnchorPos(hiddenPos, duration).SetEase(Ease.InBack));
        seq.Join(rectTransform.DOScale(hiddenScale, duration).SetEase(Ease.InBack));

        seq.OnComplete(() =>
        {
            if (parallax != null)
            {
                parallax.SetBasePosition(rectTransform.anchoredPosition);
                parallax.enabled = true;
            }

            showButton.gameObject.SetActive(true);
        });

        isShowing = false;
    }
}