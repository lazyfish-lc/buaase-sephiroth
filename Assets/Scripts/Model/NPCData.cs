using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPCStaticData")]
public class NPCStaticData : SmallObjectStaticData {
    public float interactionRange = 3f;
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
}

[Serializable]
public class NPCDynamicState : SmallObjectDynamicState {
    public bool isInConversation = false;
    public int currentNodeIndex = 0;
    public PlayerSmallObject currentInteractingPlayer;
}