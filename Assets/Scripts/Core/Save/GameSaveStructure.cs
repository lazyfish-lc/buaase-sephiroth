using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PropertySaveEntry {
    public string name;
    public float value;
}

[Serializable]
public class SmallObjectSaveData {
    public string id;
    public string staticDataName;
    public string objectType;
    public string sceneName;
    public Vector3 position;
    public Quaternion rotation;
    public bool isActive;
    public bool isDestroyed;
    public List<PropertySaveEntry> properties = new List<PropertySaveEntry>();
    public List<LabelSaveData> labels = new List<LabelSaveData>();
    public PlayerSaveData player;
    public MonsterSaveData monster;
    public NPCSaveData npc;
    public DoorSaveData door;
}

[Serializable]
public class LabelSaveData {
    public string labelType;
    public bool lockIsLocked;
    public Color glowColor;
    public float glowIntensity;
    public float glowRange;
    public int glowLightType;
    public Vector3 glowLocalPosition;
    // NPC 标签专用字段：覆盖配置的资产名称
    public string npcOverrideConfigName;
}

[Serializable]
public class PlayerSaveData {
    public float lastAttackTime;
    public bool isRunning;
    public Orientation facingDirection;
    public List<LabelSaveData> labelBackpack = new List<LabelSaveData>();
    public List<ItemSaveData> itemBackpack = new List<ItemSaveData>();
    public List<string> itemBackpackLegacy = new List<string>();
}

[Serializable]
public class ItemSaveData {
    public string itemName;
    public ItemType itemType;
    public float recoverAmount;
    public string extraJson;
}

[Serializable]
public class MonsterSaveData {
    public float lastAttackTime;
    public Orientation currentOrientation;
}

[Serializable]
public class NPCSaveData {
    public bool isInConversation;
    public int currentNodeIndex;
}

[Serializable]
public class DoorSaveData {
    public bool isOpen;
}

[Serializable]
public class Chapter3ClueSaveData {
    // 原有线索状态（向后兼容）
    public bool hasIceWeapon;
    public bool hasClockCorrected;
    public bool hasDiaryRevealed;
    public bool accusationPromptShown;

    // 多阶段指认系统 (Stage 0=大厅, 1=妻子, 2=作家, 3=助手, 4=真相)
    public int endingStage;              // 当前调查进度阶段 (0-5, 5=全部完成)
    public bool stage0Prompted;          // 大厅阶段指认面板是否已弹出
    public bool stage1Prompted;          // 妻子阶段指认面板是否已弹出
    public bool stage2Prompted;          // 作家阶段指认面板是否已弹出
    public bool stage3Prompted;          // 助手阶段指认面板是否已弹出
    public bool stage4Prompted;          // 真相阶段指认面板是否已弹出
    public bool endingTriggered;         // 是否已触发结局
    public int triggeredEndingIndex = -1; // 触发的结局索引 (0=A~4=E, -1=未触发)
    public bool fakeLabelRemoved;        // 虚伪标签是否已被移除
    public bool assistantRoomEntered;    // 是否已进入助手房间
}

[Serializable]
public class GameSavePackage {
    public string sceneName;
    public bool isPresentTime; // 当前是现在还是过去
    public List<SmallObjectSaveData> objectStates = new List<SmallObjectSaveData>();
    public List<string> triggeredTutorialIds = new List<string>();
    public List<int> visibleTutorialIndices = new List<int>();
    public Chapter3ClueSaveData chapter3ClueData; // 第三章线索触发进度
}