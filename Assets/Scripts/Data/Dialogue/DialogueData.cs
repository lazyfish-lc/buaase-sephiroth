using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueOption {
    public string text;             // 选项文字
    public int targetNodeIndex;     // 跳转目标节点 (-1表示结束)
    public string rewardProperty;   // 赠予的物品/数值名称 (如 "Gold")
    public int rewardAmount;        // 赠予数量
}

[Serializable]
public class DialogueNode {
    public string npcContent;       // NPC说的内容
    public bool hasOptions;         // 是否包含选项（若无则点击背景进入下一句）
    public int nextNodeIndex;       // hasOptions为false时的默认跳转节点
    public List<DialogueOption> options;
    public List<DialogueNodeActionSO> enterActions = new List<DialogueNodeActionSO>();
}