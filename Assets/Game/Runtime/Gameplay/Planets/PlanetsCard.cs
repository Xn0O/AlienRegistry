using System;
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
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectPic;

    [Header("Icon引用")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private string iconFolder = "PlanetIcons"; // iconName 为空目录前缀时会尝试 PlanetIcons/{iconName}

    [Header("悬浮和选中颜色设置")]
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color pressedColor = new Color(0.6f, 0.9f, 1f, 0.75f);

    public event Action<PlanetsCard, PlanetData> Clicked;

    private PlanetData data;
    private bool pointerInside;

    private void Awake()
    {
        HideSelectPic();
    }

    public void Bind(PlanetData planetData)
    {
        data = planetData;

        if (data == null)
        {
            if (textName) textName.text = string.Empty;
            if (textDescription) textDescription.text = string.Empty;
            if (iconImage) iconImage.sprite = defaultIcon;
            HideSelectPic();
            return;
        }

        if (textName) textName.text = data.name;
        if (textDescription) textDescription.text = data.description;
        if (iconImage) iconImage.sprite = LoadIcon(data.iconName);
        HideSelectPic();
    }

    private Sprite LoadIcon(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return defaultIcon;

        // 先尝试直接路径
        Sprite sp = Resources.Load<Sprite>(iconName);
        if (sp != null) return sp;

        // 再尝试约定目录
        sp = Resources.Load<Sprite>(iconFolder + "/" + iconName);
        if (sp != null) return sp;

        return defaultIcon;
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
