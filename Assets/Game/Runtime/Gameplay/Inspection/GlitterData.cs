using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlitterData : MonoBehaviour
{
    [Header("外观焦点位置")] public Vector2 appearancePosition;
    [Header("图片缩放系数")] public float scale = 1;

    [Header("闪光点表述")] [TextArea] public string glitterDescription;
}