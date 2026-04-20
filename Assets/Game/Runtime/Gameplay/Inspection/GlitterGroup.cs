using UnityEngine;
using UnityEngine.UI;

public class GlitterGroup : MonoBehaviour
{
    private void Start()
    {
        // 获取父节点下所有的 GlitterData 组件（包括隐藏的子节点）
        GlitterData[] allGlitters = GetComponentsInChildren<GlitterData>(true);
        for (int i = 0; i < allGlitters.Length; i++)
        {
            GlitterData glitterData = allGlitters[i];
            Button btn = glitterData.GetComponent<Button>();

            if (btn != null)
            {
                // 清除可能存在的旧监听器，防止重复绑定
                btn.onClick.RemoveAllListeners();

                // 动态绑定点击事件，将当前的 glitterData 传给处理函数
                int idx = i;
                btn.onClick.AddListener(() => OnGlitterClicked(idx, glitterData));
            }
            else
            {
                Debug.LogWarning($"节点 {glitterData.gameObject.name} 上没有找到 Button 组件，无法绑定点击事件！");
            }
        }

        foreach (GlitterData glitterData in allGlitters)
        {
            // 尝试在同一个 GameObject 上获取 Button 组件
        }
    }

    /// <summary>
    /// 当任意一个闪光点按钮被点击时触发
    /// </summary>
    /// <param name="data">被点击的闪光点数据</param>
    private void OnGlitterClicked(int idx, GlitterData data)
    {
        if (InspectionManager.Instance != null)
        {
            InspectionManager.Instance.OnGlitterClicked(idx, data);
        }
        else
        {
            Debug.LogError("场景中找不到 InspectionManager 的单例！");
        }
    }
}