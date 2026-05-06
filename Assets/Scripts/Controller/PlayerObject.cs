using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class PlayerSmallObject : SmallObject {
    public Rigidbody2D rb;
    public PlayerView playerView;
    public PlayerObjectDynamicState playerState => dynamicState as PlayerObjectDynamicState;
    public PlayerObjectStaticData playerStaticData => staticData as PlayerObjectStaticData;

    public LayerMask enemyLayer; // 在 Inspector 中设置为 Enemy 层
    public Transform hitPoint;   // 挂载在角色前方的一个空物体

    public Transform sensorPivot; // 用于旋转攻击范围的父物体

    public Vector2 linearVelocity;
    public void Update()
    {
        linearVelocity = rb.linearVelocity;
    }

    protected override void Awake() {
        base.Awake();
        if (playerStaticData != null && playerStaticData.labelBackpackBlueprints != null) {
            if (playerState.labelBackpack == null) playerState.labelBackpack = new List<ObjectLabel>();
            foreach (var desc in playerStaticData.labelBackpackBlueprints) {
                if (string.IsNullOrWhiteSpace(desc)) continue;
                try {
                    var lbl = LabelFactory.Build(desc);
                    if (lbl != null) playerState.labelBackpack.Add(lbl);
                } catch (Exception ex) {
                    Debug.LogWarning($"无法根据 blueprint 创建 Label '{desc}': {ex.Message}");
                }
            }
        }
        if (playerStaticData != null && playerStaticData.itemBackpackBlueprints != null) {
            if (playerState.itemBackpack == null) playerState.itemBackpack = new List<Item>();
            foreach (var item in playerStaticData.itemBackpackBlueprints) {
                if (item == null || string.IsNullOrWhiteSpace(item.itemName)) continue;
                playerState.itemBackpack.Add(new Item { itemName = item.itemName });
            }
        }
    }
    

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new PlayerObjectDynamicState();
    }

     public override void Start() {
        base.Start();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public override void OnMoveAction(Vector3 moveDir) {
        Vector3 direction = Vector3.zero;
        direction = moveDir.normalized;
        rb.linearVelocity = new Vector2(direction.x * playerState.propertyMap["moveSpeed"].value, direction.y * playerState.propertyMap["moveSpeed"].value);
        playerState.isRunning = rb.linearVelocity.magnitude > 0.1f;
        UpdateOrientation(moveDir);
        playerView.UpdateMovement(GetAnimationDirection(), playerState.isRunning);

    }

    public override void OnInteractAction(InputEventData data) {
        Debug.Log("玩家尝试自我交互（如打开背包）");
    }

    public void Attack() {
        if (playerState == null || playerStaticData == null) {
            Debug.LogWarning("玩家状态或静态数据类型配置错误，无法攻击");
            return;
        }

        float attackSpeed = playerState.propertyMap.ContainsKey("AttackSpeed") ? playerState.propertyMap["AttackSpeed"].value : 1f;
        float cooldown = playerStaticData.baseAttackCooldown / attackSpeed;
        if (Time.time - playerState.lastAttackTime >= cooldown) {
            playerState.lastAttackTime = Time.time;
            Debug.Log("玩家攻击！");
            // 这里可以添加攻击逻辑，比如检测附近的敌人并造成伤害
            playerView.PlayAttack(playerState.isRunning);
        } else {
            Debug.Log("攻击冷却中...");
        }
    }

    public void UpdateOrientation(Vector3 moveDir) {
        if (moveDir.magnitude > 0.1f) {
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
                playerState.facingDirection = moveDir.x > 0 ? Orientation.Right : Orientation.Left;
            } else {
                playerState.facingDirection = moveDir.y > 0 ? Orientation.Up : Orientation.Down;
            }
        }

        // 修改传感器子物体的旋转，使碰撞体朝向玩家
        float angle = 0;
        switch (playerState.facingDirection) {
            case Orientation.Up: angle = 180; break;
            case Orientation.Down: angle = 0; break;
            case Orientation.Left: angle = 270; break;
            case Orientation.Right: angle = 90; break;
        }
        sensorPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2 GetAnimationDirection() {
        if (playerState.facingDirection == Orientation.Up) return new Vector2(0, 1);
        if (playerState.facingDirection == Orientation.Down) return new Vector2(0, -1);
        if (playerState.facingDirection == Orientation.Left) return new Vector2(-1, 0);
        if (playerState.facingDirection == Orientation.Right) return new Vector2(1, 0);
        return new Vector2(0, -1); // 默认朝下
    }

    public void ExecuteDamageDetection() {
        // 1. 获取当前攻击力数值（符合核心玩法1）
        float currentAtk = playerState.propertyMap.ContainsKey("ATK") ? playerState.propertyMap["ATK"].value : 10;

        // 2. 物理探测（判定范围）
        float hitRadius = 0.8f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitPoint.position, hitRadius, enemyLayer);

        // 3. 封装信息并分发
        foreach (var targetCollider in hitTargets) {
            SmallObject target = targetCollider.GetComponent<SmallObject>();
            if (target != null) {
                // 计算击退方向：从攻击者指向被攻击者
                Vector2 knockback = (target.transform.position - transform.position).normalized * 50f;
                
                DamagePacket packet = new DamagePacket(this, currentAtk, knockback);
                
                // --- 核心步骤：信息传递 ---
                target.ReceiveDamage(packet);
            }
        }
    }

    public override bool IsEnemy(SmallObject other) {
        // 玩家认为所有 MonsterObject 都是敌人
        return other.gameObject.layer == enemyLayer || other is DoorSmallObject; 
    }

    public void AddItemToBackpack(Item item) {
        if (item == null) return;
        playerState.itemBackpack.Add(item);
        Debug.Log($"玩家获得了物品: {item.itemName}");
    }

    public bool containsTruthLens() {
        foreach (var item in playerState.itemBackpack) {
            if (item.itemName == "TruthLens") {
                return true;
            }
        }
        return false;
    }
}