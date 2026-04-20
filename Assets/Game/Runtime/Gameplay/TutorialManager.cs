using Game.Runtime.Core;
using Game.Runtime.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Parts")] public InspectionPanel inspectionPanel;
    public PadPanel padPanel;
    public PackageView packageView;
    public GlitterView glitterView;

    public GameObject cover;


    private void Start()
    {
        cover.SetActive(true);
    }

    public void LoadRound(int index)
    {
        inspectionPanel.progressText.text = $"";
        inspectionPanel.ResetQuestionTexts();
    }


    private async Task UpdateCharacterDisplay(CharacterData data)
    {

        Task t1 = inspectionPanel.PlayWalkAsync(false);
        Task t2 = inspectionPanel.ArmExtendAsync();

        await Task.WhenAll(t1, t2);
        Debug.Log("入场动画播完了，玩家现在可以开始操作了");
        cover.SetActive(false);
    }

    private async Task NextRoundSequenceAsync()
    {
        // 1. 同时开启两个动画，但不立即 await 它们
        // 这会让机械臂收回和角色走开【同时开始】
        Task armTask = inspectionPanel.ArmRetractAsync();
        Task walkTask = inspectionPanel.PlayWalkAsync(true);

        // 2. 等待两个任务全部完成
        // 即使一个播 1s，一个播 2s，代码也会等最长的那个播完
        await Task.WhenAll(armTask, walkTask);
    }
}