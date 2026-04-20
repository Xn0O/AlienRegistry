using System;
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

    [Header("Icon引用")]
    [SerializeField] private Sprite defaultIcon;
    public Sprite[] planetSprites;

    [Header("悬浮和选中颜色设置")]
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color pressedColor = new Color(0.6f, 0.9f, 1f, 0.75f);

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

    private void Awake()
    {
        BuildIconDictionary();
        HideSelectPic();
    }

    private void OnDisable()
    {
        // 防止面板隐藏后残留按下状态
        pointerInside = false;
        pointerDown = false;
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

        if (lockedSelected) ShowSelectPic(pressedColor);
        else HideSelectPic();
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

        // 按住状态下回到卡片仍显示按下色，松开后才回 Hover
        if (pointerDown) ShowSelectPic(pressedColor);
        else ShowSelectPic(hoverColor);
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
        ShowSelectPic(pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactionEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 只有松开后才切回 HoverColor
        pointerDown = false;

        if (pointerInside) ShowSelectPic(hoverColor);
        else HideSelectPic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactionEnabled) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (data == null) return;
        Clicked?.Invoke(this, data);
    }

    private void ShowSelectPic(Color color)
    {
        if (selectPic == null) return;
        selectPic.gameObject.SetActive(true);
        selectPic.color = color;
    }

    private void HideSelectPic()
    {
        if (selectPic == null) return;
        selectPic.gameObject.SetActive(false);
    }
}
