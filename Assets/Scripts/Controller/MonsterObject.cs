using UnityEngine;

public class MonsterSmallObject : SmallObject {
    public MonsterView monsterView;
    public Transform sensorPivot;
    public MonsterSensor detectSensor;
    public MonsterSensor attackSensor;
    public MonsterObjectStaticData monsterStaticData => staticData as MonsterObjectStaticData;
    public MonsterObjectDynamicState monsterState => dynamicState as MonsterObjectDynamicState;

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
        if (monsterState == null) {
            Debug.LogWarning("怪物状态类型配置错误，无法修改数值");
            return;
        }

        // 修改血量
        monsterState.currentHealth += delta * 50f;
        Debug.Log($"怪物 {staticData.objectName} 血量变更为: {monsterState.currentHealth}");
        
        // 此处可触发【核心玩法1：数值守恒】
        // ValueManager.Instance.ApplyConservation(this, delta);
    }

    public override void OnInteractAction() {
        Debug.Log("对怪物进行交互（如：偷窃标签）");
    }

    private void Update() {
        HandleAIBehavior();
    }

    private void HandleAIBehavior() {
        if (monsterState.isDestroyed) return;
        if (monsterView.IsPlayingAttack()) return; // 攻击动画播放中，暂不处理AI逻辑
        // Debug.Log($"怪物 {staticData.objectName} AI 处理，目标玩家: {(monsterState.targetPlayer != null ? monsterState.targetPlayer.name : "无")}, 是否在攻击范围: {monsterState.isPlayerInAttackRange}");
        if (monsterState.targetPlayer != null) {
            // 1. 计算朝向
            Vector3 direction = (monsterState.targetPlayer.position - transform.position).normalized;
            UpdateMonsterOrientation(direction);

            // 2. 攻击判断
            if (monsterState.isPlayerInAttackRange) {
                
                TryAttack();
                
            } else {
                // 3. 移动逻辑
                rb.linearVelocity = new Vector2(direction.x, direction.y) * monsterState.moveSpeed;
                monsterView.UpdateMovement(new Vector2(direction.x, direction.y), true);
            }
        } else {
            monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false); // 无目标时静止
        }
    }

    private void UpdateMonsterOrientation(Vector3 dir) {
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

    private void TryAttack() {
        float attackSpeed = monsterState.propertyMap.ContainsKey("AttackSpeed") ? monsterState.propertyMap["AttackSpeed"].value : 1.0f;
        float interval = monsterStaticData.baseAttackCooldown / attackSpeed;

        if (Time.time >= monsterState.lastAttackTime + interval) {
            monsterState.lastAttackTime = Time.time;
            monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false); // 攻击时停止移动
            rb.linearVelocity = Vector2.zero; // 攻击时停止移动
            monsterView.PlayAttack();
        }
    }

    private Vector2 OrientationToVector(Orientation orientation) {
        switch (orientation) {
            case Orientation.Up: return Vector2.up;
            case Orientation.Down: return Vector2.down;
            case Orientation.Left: return Vector2.left;
            case Orientation.Right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    
}