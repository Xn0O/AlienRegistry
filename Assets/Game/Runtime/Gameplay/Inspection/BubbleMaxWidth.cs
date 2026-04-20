using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways] // 允许在编辑器里直接预览效果，无需运行
[RequireComponent(typeof(TextMeshProUGUI), typeof(LayoutElement))]
public class BubbleMaxWidth : MonoBehaviour
{
    [Header("允许的最大宽度")] public float maxWidth = 460f;

    private TextMeshProUGUI tmpText;
    private LayoutElement layoutElement;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        layoutElement = GetComponent<LayoutElement>();
    }

    void Update()
    {
        if (tmpText == null || layoutElement == null) return;

        // 判断：如果文字单行排列的理想宽度超过了最大宽度
        if (tmpText.preferredWidth > maxWidth)
        {
            // 强制限制宽度为 maxWidth，此时 TMP 遇到边界会自动向下换行
            layoutElement.preferredWidth = maxWidth;
        }
        else
        {
            // 如果文字较少，禁用宽度限制（设为-1），让气泡自由收缩
            layoutElement.preferredWidth = -1;
        }
    }
}