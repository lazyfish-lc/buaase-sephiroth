using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterObjectData", menuName = "Game/MonsterObjectData")]
public class MonsterObjectStaticData : SmallObjectStaticData {
    public float baseAttackCooldown = 1f;
    public float castLockTime = 2f; // 施法僵直时间，怪物在这个时间内无法移动或攻击
}

[Serializable]
public class MonsterObjectDynamicState : SmallObjectDynamicState {
    public float moveSpeed = 2f;
    public float lastAttackTime = -Mathf.Infinity;
    public bool isPlayerInAttackRange = false;
    public Transform targetPlayer;
    public Orientation currentOrientation = Orientation.Down;
}