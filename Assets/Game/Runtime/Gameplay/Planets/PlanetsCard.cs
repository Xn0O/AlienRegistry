using System;
using System.Collections;
using System.Collections.Generic;
using Game.Runtime.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlanetsCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textDescription;
    [SerializeField] private TextMeshProUGUI textNeed;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectPic;
    [SerializeField] private Image selectPicN;

    [Header("Icon引用")]
    [SerializeField] private Sprite defaultIcon;
    public Sprite[] planetSprites;

    [Header("选中动画设置")]
    [SerializeField] private bool useLockPopAnimation = true;
    [SerializeField] private float lockPopScale = 1.5f;
    [SerializeField] private float lockPopDuration = 0.2f;
    [SerializeField,Range(0.05f, 0.95f)] private float lockPopExpandRatio = 0.2f;

    public event Action<PlanetsCard, PlanetData> Clicked;

    private PlanetData data;
    private bool pointerInside;
    private bool pointerDown; // 是否仍在按住鼠标左键

    // 是否允许交互
    private bool interactionEnabled = true;
    // 是否为选中卡片
    private bool lockedSelected = false;

    // 缓存图标
    private readonly Dictionary<string, Sprite> iconDict = new Dictionary<string, Sprite>(StringComparer.Ordinal);
    // 保证同一个缺失图标只警告一次
    private readonly HashSet<string> missingIconWarned = new HashSet<string>(StringComparer.Ordinal);

    // selectPic 原始缩放
    private Vector3 selectPicBaseScale = Vector3.one;
    private Vector3 selectPicNBaseScale = Vector3.one;
    private Coroutine lockPopCoroutine;

    private void Awake()
    {
        BuildIconDictionary();

        if (selectPic != null)
            selectPicBaseScale = selectPic.rectTransform.localScale;
        if (selectPicN != null)
            selectPicNBaseScale = selectPicN.rectTransform.localScale;

        HideSelectPic();
    }

    private void OnDisable()
    {
        // 防止面板隐藏后残留按下状态
        pointerInside = false;
        pointerDown = false;

        StopLockPopAnimation();
        ResetSelectPicScale();
        HideSelectPic();
    }

    // 面板控制卡片可交互状态
    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;

        if (!interactionEnabled && !lockedSelected)
        {
            pointerInside = false;
            pointerDown = false;
            HideSelectPic();
        }
    }

    // 面板锁定状态
    public void SetLockedSelected(bool selected)
    {
        lockedSelected = selected;
        pointerInside = false;
        pointerDown = false;

        if (lockedSelected)
        {
            ShowSelectPic();
            PlayLockPopAnimation();
        }
        else
        {
            StopLockPopAnimation();
            ResetSelectPicScale();
            HideSelectPic();
        }
    }

    // 构建图标字典
    private void BuildIconDictionary()
    {
        iconDict.Clear();

        if (planetSprites == null) return;

        for (int i = 0; i < planetSprites.Length; i++)
        {
            var sp = planetSprites[i];
            if (sp == null) continue;
            if (string.IsNullOrWhiteSpace(sp.name)) continue;

            if (!iconDict.ContainsKey(sp.name))
            {
                iconDict.Add(sp.name, sp);
            }
        }
    }

    // 通过图标名取图标
    private Sprite GetIcon(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return defaultIcon;

        iconName = iconName.Trim();

        if (iconDict.TryGetValue(iconName, out var sp) && sp != null)
            return sp;

        // 遍历数组查找
        if (planetSprites != null)
        {
            for (int i = 0; i < planetSprites.Length; i++)
            {
                var item = planetSprites[i];
                if (item == null) continue;
                if (!string.Equals(item.name, iconName, StringComparison.Ordinal)) continue;

                iconDict[iconName] = item;
                return item;
            }
        }

        // 只警告一次
        if (!missingIconWarned.Contains(iconName))
        {
            missingIconWarned.Add(iconName);
            Debug.LogWarning($"[PlanetsCard] 找不到星球图标: {iconName}，将使用 defaultIcon。");
        }

        return defaultIcon;
    }

    public void Bind(PlanetData planetData)
    {
        data = planetData;

        // 每次绑定重置锁状态
        interactionEnabled = true;
        lockedSelected = false;
        pointerInside = false;
        pointerDown = false;

        StopLockPopAnimation();
        ResetSelectPicScale();

        if (data == null)
        {
            if (textName) textName.text = string.Empty;
            if (textDescription) textDescription.text = string.Empty;
            if (textNeed) textNeed.text = string.Empty;
            if (iconImage) iconImage.sprite = defaultIcon;
            HideSelectPic();
            return;
        }

        if (textName) textName.text = data.name;
        if (textDescription) textDescription.text = data.description;
        if (textNeed) textNeed.text = data.planetneed;
        if (iconImage) iconImage.sprite = GetIcon(data.iconName);
        HideSelectPic();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactionEnabled) return;

        pointerInside = true;
        ShowSelectPic();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactionEnabled) return;

        pointerInside = false;
        HideSelectPic();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactionEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        pointerDown = true;
        ShowSelectPic();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactionEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 只有松开后才决定是否继续显示
        pointerDown = false;

        if (pointerInside) ShowSelectPic();
        else HideSelectPic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactionEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (data == null) return;
        Clicked?.Invoke(this, data);
    }

    private void ShowSelectPic()
    {
        // 未锁定：显示 selectPic_n；锁定：显示 selectPic
        if (!lockedSelected)
        {
            if (selectPic != null) selectPic.gameObject.SetActive(false);

            if (selectPicN != null)
            {
                selectPicN.gameObject.SetActive(true);
            }
            else if (selectPic != null) // 兼容没填 selectPicN 的情况
            {
                selectPic.gameObject.SetActive(true);
            }

            return;
        }

        if (selectPicN != null) selectPicN.gameObject.SetActive(false);

        if (selectPic != null)
        {
            selectPic.gameObject.SetActive(true);
        }
        else if (selectPicN != null) // 兼容没填 selectPic 的情况
        {
            selectPicN.gameObject.SetActive(true);
        }
    }

    private void HideSelectPic()
    {
        if (selectPic != null) selectPic.gameObject.SetActive(false);
        if (selectPicN != null) selectPicN.gameObject.SetActive(false);
    }

    private void PlayLockPopAnimation()
    {
        if (!useLockPopAnimation) return;
        if (!isActiveAndEnabled) return;

        StopLockPopAnimation();
        lockPopCoroutine = StartCoroutine(CoLockPop());
    }

    private void StopLockPopAnimation()
    {
        if (lockPopCoroutine != null)
        {
            StopCoroutine(lockPopCoroutine);
            lockPopCoroutine = null;
        }
    }

    // 选中动画
    private IEnumerator CoLockPop()
    {
        RectTransform rt = null;
        Vector3 from = Vector3.one;

        if (selectPic != null)
        {
            rt = selectPic.rectTransform;
            from = selectPicBaseScale;
        }
        else if (selectPicN != null)
        {
            rt = selectPicN.rectTransform;
            from = selectPicNBaseScale;
        }

        if (rt == null)
        {
            lockPopCoroutine = null;
            yield break;
        }

        Vector3 to = from * Mathf.Max(1f, lockPopScale);

        float total = Mathf.Max(0.01f, lockPopDuration);

        float expandRatio = Mathf.Clamp(lockPopExpandRatio, 0.05f, 0.95f);
        float expandDuration = total * expandRatio;          // 放大阶段
        float shrinkDuration = total * (1f - expandRatio);  // 缩回阶段

        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = expandDuration <= 0f ? 1f : t / expandDuration;
            rt.localScale = Vector3.LerpUnclamped(from, to, p);
            yield return null;
        }

        t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = shrinkDuration <= 0f ? 1f : t / shrinkDuration;
            rt.localScale = Vector3.LerpUnclamped(to, from, p);
            yield return null;
        }

        rt.localScale = from;
        lockPopCoroutine = null;
    }

    private void ResetSelectPicScale()
    {
        if (selectPic != null)
            selectPic.rectTransform.localScale = selectPicBaseScale;

        if (selectPicN != null)
            selectPicN.rectTransform.localScale = selectPicNBaseScale;
    }
}
