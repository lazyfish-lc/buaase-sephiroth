using System;
using UnityEngine;

using System.Collections.Generic;

public enum Orientation { Up, Down, Left, Right }
[CreateAssetMenu(fileName = "PlayerObjectData", menuName = "Game/PlayerObjectData")]
public class PlayerObjectStaticData : SmallObjectStaticData {
    public float baseAttackCooldown = 0.5f;
}

[Serializable]
public class PlayerObjectDynamicState : SmallObjectDynamicState {
    public float moveSpeed = 5f;

    public float lastAttackTime = -Mathf.Infinity;
    public bool isRunning = false;
    public Orientation facingDirection = Orientation.Down;
    public List<ObjectLabel> labelBackpack = new List<ObjectLabel>();
}