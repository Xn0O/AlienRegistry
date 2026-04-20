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

    private int currentId = -1;
    private Tween currentTween;

    private RectTransform rect;

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
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     点击某个点
    /// </summary>
    public void Show(int id, Sprite sprite, string desc)
    {
        // 👉 如果点的是同一个，直接无视
        if (currentId == id && gameObject.activeSelf)
        {
            return;
        }

        currentId = id;

        // 👉 如果当前已经打开，先收再开
        if (gameObject.activeSelf)
        {
            Hide(() =>
            {
                SetContent(sprite, desc);
                PlayShow();
            });
        }
        else
        {
            gameObject.SetActive(true);
            SetContent(sprite, desc);
            PlayShow();
        }
    }

    private void SetContent(Sprite sprite, string desc)
    {
        glitterPortrait.sprite = sprite;
        glitterDesc.text = desc;
    }

    private void PlayShow()
    {
        currentTween?.Kill();

        rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);

        currentTween = rect.DOAnchorPosX(-400f, duration)
            .SetEase(Ease.OutCubic);
    }

    /// <summary>
    ///     点击关闭按钮
    /// </summary>
    public void OnClickClose()
    {
        currentId = -1;
        Hide();
    }

    private void Hide(Action onComplete = null)
    {
        currentTween?.Kill();

        currentTween = rect.DOAnchorPosX(0f, duration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}