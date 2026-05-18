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
}

[Serializable]
public class PlayerSaveData {
    public float lastAttackTime;
    public bool isRunning;
    public Orientation facingDirection;
    public List<LabelSaveData> labelBackpack = new List<LabelSaveData>();
    public List<string> itemBackpack = new List<string>();
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
public class GameSavePackage {
    public string sceneName;
    public bool isPresentTime; // 当前是现在还是过去
    public List<SmallObjectSaveData> objectStates = new List<SmallObjectSaveData>();
}