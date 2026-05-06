using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPCStaticData")]
public class NPCStaticData : SmallObjectStaticData {
    public float interactionRange = 3f;
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
    // 要求玩家身上必须拥有的物品名（例如钥匙的三部分）
    public List<string> requiredItems = new List<string>();
    // 当玩家满足 requiredItems 时，从该节点开始对话（-1 表示使用默认 0）
    public int startNodeIfHasRequiredItems = -1;
    public RuntimeAnimatorController animatorController;
}

[Serializable]
public class NPCDynamicState : SmallObjectDynamicState {
    public bool isInConversation = false;
    public int currentNodeIndex = 0;
    public PlayerSmallObject currentInteractingPlayer;
}