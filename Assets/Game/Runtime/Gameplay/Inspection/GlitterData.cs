using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GliterData : MonoBehaviour
{
    [Header("外观焦点位置")] public Vector2 appearancePosition;
    [Header("图片缩放系数")] public float scale = 1;

    [Header("闪光点表述")] [TextArea] public string glitterDescription;

    [Header("目标图片空间")] public Image targetImage;
    [Header("文字描述控件")] public TMP_Text targetDesc;

    private void OnValidate()
    {
        if (targetImage)
        {
            RectTransform rect = targetImage.rectTransform;
            rect.anchoredPosition = appearancePosition;
            rect.localScale = Vector3.one * scale;
        }

        if (targetDesc)
        {
            targetDesc.text = glitterDescription;
        }
    }
}