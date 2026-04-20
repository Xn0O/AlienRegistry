using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlitterView : MonoBehaviour
{
    [SerializeField] private Image glitterPortrait;
    [SerializeField] private TMP_Text glitterDesc;
    public Button CloseButton;
    public float duration = 0.3f;

    private Tween currentTween;

    private RectTransform rect;
    private RectTransform portraitRect;
    private GlitterData currentData;
    private bool isOpen;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (glitterPortrait == null)
        {
            glitterPortrait = transform.Find("Mask/GlitterPortrait").GetComponent<Image>();
        }

        if (glitterDesc == null)
        {
            glitterDesc = transform.Find("GlitterDesc").GetComponent<TMP_Text>();
        }

        CloseButton.onClick.RemoveAllListeners();
        CloseButton.onClick.AddListener(() => OnClickClose());
    }

    private void Start()
    {
        rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);
        isOpen = false;
    }

    /// <summary>
    ///     点击某个点
    /// </summary>
    public void Show(Sprite sprite, GlitterData data)
    {
        //如果点的是同一个，直接无视
        if (currentData == data)
        {
            return;
        }

        currentData = data;

        // 如果当前已经打开，先收再开
        if (isOpen)
        {
            Hide(() =>
            {
                SetContent(sprite, data);
                PlayShow();
            });
        }
        else
        {
            SetContent(sprite, data);
            PlayShow();
        }
    }

    private void SetContent(Sprite sprite, GlitterData data)
    {
        glitterPortrait.sprite = sprite;
        glitterPortrait.rectTransform.anchoredPosition = data.appearancePosition;
        glitterPortrait.rectTransform.localScale = Vector2.one * data.scale;
        glitterDesc.text = data.glitterDescription;
    }

    private void PlayShow()
    {
        currentTween?.Kill();

        rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);

        currentTween = rect.DOAnchorPosX(-400f, duration)
            .SetEase(Ease.OutCubic);

        CloseButton.gameObject.SetActive(true);
        isOpen = true;
    }

    /// <summary>
    ///     点击关闭按钮
    /// </summary>
    public void OnClickClose()
    {
        isOpen = false;
        currentData = null;
        Hide();
    }

    private void Hide(Action onComplete = null)
    {
        currentTween?.Kill();

        currentTween = rect.DOAnchorPosX(0f, duration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                isOpen = false;
                onComplete?.Invoke();
            });
    }
}