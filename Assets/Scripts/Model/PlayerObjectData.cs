using System;
using UnityEngine;

using System.Collections.Generic;

public enum Orientation { Up, Down, Left, Right }
[CreateAssetMenu(fileName = "PlayerObjectData", menuName = "Game/PlayerObjectData")]
public class PlayerObjectStaticData : SmallObjectStaticData {
    public float baseAttackCooldown = 0.5f;

    public List<string> labelBackpackBlueprints = new List<string>();
    public List<Item> itemBackpackBlueprints = new List<Item>();
}

[Serializable]
public class PlayerObjectDynamicState : SmallObjectDynamicState {

    public float lastAttackTime = -Mathf.Infinity;
    public bool isRunning = false;
    public Orientation facingDirection = Orientation.Down;
    public List<ObjectLabel> labelBackpack = new List<ObjectLabel>();
    public List<Item> itemBackpack = new List<Item>();
}