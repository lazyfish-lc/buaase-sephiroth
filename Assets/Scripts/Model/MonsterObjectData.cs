using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterObjectData", menuName = "Game/MonsterObjectData")]
public class MonsterObjectStaticData : SmallObjectStaticData {
    public float baseAttackCooldown = 1f;
}

[Serializable]
public class MonsterObjectDynamicState : SmallObjectDynamicState {
    public float moveSpeed = 2f;
    public float lastAttackTime = -Mathf.Infinity;
    public bool isPlayerInAttackRange = false;
    public Transform targetPlayer;
    public Orientation currentOrientation = Orientation.Down;
}