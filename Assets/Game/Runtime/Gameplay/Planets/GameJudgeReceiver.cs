using UnityEngine;

public class GameJudgeReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        PlanetsPanel.AddJudgeResultListener(OnJudgeResult); // 订阅
    }

    private void OnDisable()
    {
        PlanetsPanel.RemoveJudgeResultListener(OnJudgeResult); // 取消订阅
    }

    private void OnJudgeResult(bool isCorrect, int planetId)
    {
        Debug.Log($"收到结果: isCorrect={isCorrect}, planetId={planetId}");

        if (isCorrect)
        {
            Debug.Log("选择正确处理逻辑");
        }
        else
        {
            Debug.Log("选择错误处理逻辑");
        }
    }
}
