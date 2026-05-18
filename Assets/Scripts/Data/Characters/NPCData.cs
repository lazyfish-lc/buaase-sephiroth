using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPCStaticData")]
public class NPCStaticData : SmallObjectStaticData {
    public float interactionRange = 7f;
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
    // 可配置的物品组：若玩家包含组内全部物品，则从对应节点开始对话
    [Serializable]
    public class RequiredItemGroup {
        public List<string> requiredItems = new List<string>();
        // 当玩家满足 requiredItems 时，从该节点开始对话（-1 表示使用默认 0）
        public int startNodeIfHasRequiredItems = -1;
    }

    public List<RequiredItemGroup> requiredItemGroups = new List<RequiredItemGroup>();
    public RuntimeAnimatorController animatorController;
}

[Serializable]
public class NPCDynamicState : SmallObjectDynamicState {
    public bool isInConversation = false;
    public int currentNodeIndex = 0;
    public PlayerSmallObject currentInteractingPlayer;
}