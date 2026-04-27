using UnityEngine;

public class MonsterSmallObject : SmallObject {
    public MonsterView monsterView;
    public Transform sensorPivot;
    public MonsterSensor detectSensor;
    public MonsterSensor attackSensor;
    public MonsterObjectStaticData monsterStaticData => staticData as MonsterObjectStaticData;
    public MonsterObjectDynamicState monsterState => dynamicState as MonsterObjectDynamicState;

    public LayerMask playerLayer; // 在 Inspector 中设置为 Enemy 层
    public Transform hitPoint;   // 挂载在角色前方的一个空物体

    public Rigidbody2D rb;

    protected override void Awake() {
        base.Awake();
        if (monsterView == null) {
            monsterView = GetComponentInChildren<MonsterView>();
            if (monsterView == null) Debug.LogWarning("未找到 MonsterView 组件");
        }
        detectSensor.onEnter = (col) => monsterState.targetPlayer = col.transform;
        detectSensor.onExit = (col) => monsterState.targetPlayer = null;
        attackSensor.onEnter = (col) => monsterState.isPlayerInAttackRange = true;
        attackSensor.onExit = (col) => monsterState.isPlayerInAttackRange = false;
        rb = GetComponent<Rigidbody2D>();
    }
    
    protected override SmallObjectDynamicState CreateDynamicState() {
        return new MonsterObjectDynamicState();
    }
    
    public override void OnValueModifyAction(string prop, float delta) {
        // TODO
    }

    public override void OnInteractAction(InputEventData data) {
        Debug.Log("对怪物进行交互（如：偷窃标签）");
    }

    private void Update() {
        HandleAIBehavior();
    }

    protected virtual void HandleAIBehavior() {
        if (monsterState.isDestroyed) return;
        if (monsterView.IsPlayingAttack()) return; // 攻击动画播放中，暂不处理AI逻辑
        if (monsterState.isHurt) return; // 受击状态下暂不处理AI逻辑
        // Debug.Log($"怪物 {staticData.objectName} AI 处理，目标玩家: {(monsterState.targetPlayer != null ? monsterState.targetPlayer.name : "无")}, 是否在攻击范围: {monsterState.isPlayerInAttackRange}");
        if (monsterState.targetPlayer != null) {
            // 1. 计算朝向
            Vector3 direction = (monsterState.targetPlayer.position - transform.position).normalized;
            UpdateMonsterOrientation(direction);

            // 2. 攻击判断
            if (monsterState.isPlayerInAttackRange) {
                bool attackedThisFrame = TryAttack();
                if (!attackedThisFrame) {
                    // 冷却期间保持追击，避免在攻击范围边缘呆站。
                    rb.linearVelocity = new Vector2(direction.x, direction.y) * monsterState.moveSpeed;
                    monsterView.UpdateMovement(new Vector2(direction.x, direction.y), true);
                }
            } else {
                // 3. 移动逻辑
                rb.linearVelocity = new Vector2(direction.x, direction.y) * monsterState.moveSpeed;
                monsterView.UpdateMovement(new Vector2(direction.x, direction.y), true);
            }
        } else {
            monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false); // 无目标时静止
        }
    }

    protected void UpdateMonsterOrientation(Vector3 dir) {
        // 更新朝向枚举
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
            monsterState.currentOrientation = dir.x > 0 ? Orientation.Right : Orientation.Left;
        } else {
            monsterState.currentOrientation = dir.y > 0 ? Orientation.Up : Orientation.Down;
        }

        // 修改传感器子物体的旋转，使碰撞体朝向玩家
        float angle = 0;
        switch (monsterState.currentOrientation) {
            case Orientation.Up: angle = 180; break;
            case Orientation.Down: angle = 0; break;
            case Orientation.Left: angle = 270; break;
            case Orientation.Right: angle = 90; break;
        }
        sensorPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected virtual bool TryAttack() {
        float attackSpeed = monsterState.propertyMap.ContainsKey("AttackSpeed") ? monsterState.propertyMap["AttackSpeed"].value : 1.0f;
        float interval = monsterStaticData.baseAttackCooldown / attackSpeed;

        if (Time.time >= monsterState.lastAttackTime + interval) {
            monsterState.lastAttackTime = Time.time;
            monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false); // 攻击时停止移动
            rb.linearVelocity = Vector2.zero; // 攻击时停止移动
            monsterView.PlayAttack();
            return true;
        }

        return false;
    }

    protected Vector2 OrientationToVector(Orientation orientation) {
        switch (orientation) {
            case Orientation.Up: return Vector2.up;
            case Orientation.Down: return Vector2.down;
            case Orientation.Left: return Vector2.left;
            case Orientation.Right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    public void ExecuteDamageDetection() {
        // 1. 获取当前攻击力数值（符合核心玩法1）
        float currentAtk = monsterState.propertyMap.ContainsKey("ATK") ? monsterState.propertyMap["ATK"].value : 10;

        // 2. 物理探测（判定范围）
        float hitRadius = 0.8f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitPoint.position, hitRadius, playerLayer);

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
        // 怪物认为玩家是敌人
        Debug.Log($"{staticData.objectName} 判断 {other.staticData.objectName} 是否为敌人: {1 << other.gameObject.layer == playerLayer.value}");
        return 1 << other.gameObject.layer == playerLayer.value;
    }
}   