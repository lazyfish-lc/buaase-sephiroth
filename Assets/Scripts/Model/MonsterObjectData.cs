using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MonsterObjectData", menuName = "Game/MonsterObjectData")]
public class MonsterObjectStaticData : SmallObjectStaticData {
    public float baseAttackCooldown = 1f;
    public float castLockTime = 2f; // 施法僵直时间，怪物在这个时间内无法移动或攻击
    // 击杀后掉落：每个元素表示一个物品名称的 blueprint，会向击杀者背包添加一件对应名称的物品
    public List<Item> dropItemBlueprints = new List<Item>();
}

[Serializable]
public class MonsterObjectDynamicState : SmallObjectDynamicState {
    public float lastAttackTime = -Mathf.Infinity;
    public bool isPlayerInAttackRange = false;
    public Transform targetPlayer;
    public Orientation currentOrientation = Orientation.Down;
    // 记录最后造成伤害的攻击者，以便在死亡时发放掉落
    public SmallObject lastAttacker;
}