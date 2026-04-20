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

    // 名称 -> Sprite 的运行时缓存
    private readonly Dictionary<string, Sprite> iconDict = new Dictionary<string, Sprite>(StringComparer.Ordinal);
    // 保证同一个缺失图标只警告一次
    private readonly HashSet<string> missingIconWarned = new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        BuildIconDictionary();
        HideSelectPic();
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

        // 仅警告一次
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
        pointerInside = true;
        ShowSelectPic(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        HideSelectPic();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        ShowSelectPic(pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (pointerInside) ShowSelectPic(hoverColor);
        else HideSelectPic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
