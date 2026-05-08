using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public abstract class SmallObject : MonoBehaviour, ILabelOwner {
    
    public SmallObjectStaticData staticData;
    public SmallObjectDynamicState dynamicState;
    public BigObject ownerBigObject;
    
    // Events that labels can subscribe to
    public event Action<ILabelOwner> LabelOnAttacking;
    public event Action<ILabelOwner> LabelOnCrash;
    public event Action<ILabelOwner> LabelOnAttacked;
    public event Action LabelOnTick;
    public event Action LabelOnMoving;

    public event Action<SmallObject> OnShowLabel;
    public event Action<SmallObject> OnShowProperty;

    public string persistID; // 用于存档系统唯一标识该 SmallObject 实例

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrWhiteSpace(persistID)) {
            persistID = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
    protected virtual void Awake() {
        if (string.IsNullOrWhiteSpace(persistID)) {
            persistID = System.Guid.NewGuid().ToString("N");
            Debug.LogWarning($"{name} 缺少 persistID，已在运行时生成：{persistID}");
        }
        dynamicState = CreateDynamicState();
        if (dynamicState == null) {
            dynamicState = new SmallObjectDynamicState();
            Debug.LogWarning($"{GetType().Name} CreateDynamicState 返回空，已回退为 SmallObjectDynamicState");
        }
        InitializeProperties();
        // 根据静态数据中的 labelBlueprints 初始化标签（使用 LabelFactory 通过字符串创建实例）
        if (staticData != null && staticData.labelBlueprints != null) {
            if (dynamicState.smallObjectLabels == null) dynamicState.smallObjectLabels = new List<ObjectLabel>();
            foreach (var desc in staticData.labelBlueprints) {
                if (string.IsNullOrWhiteSpace(desc)) continue;
                try {
                    var lbl = LabelFactory.Build(desc);
                    if (lbl != null) dynamicState.smallObjectLabels.Add(lbl);
                } catch (Exception ex) {
                    Debug.LogWarning($"无法根据 blueprint 创建 Label '{desc}': {ex.Message}");
                }
            }
        }
        // Attach any labels stored in dynamic state
        if (dynamicState != null && dynamicState.smallObjectLabels != null) {
            foreach (var label in dynamicState.smallObjectLabels) label?.AttachToOwner(this);
        }
    }

    protected virtual SmallObjectDynamicState CreateDynamicState() {
        return new SmallObjectDynamicState();
    }

    public virtual void Start() {
        if (dynamicState == null) {
            dynamicState = CreateDynamicState();
        }
    }

    private void InitializeProperties() {
        // 第一步：实例化所有定义的属性（确保后面规则引用时属性已存在）
        foreach (var blueprint in staticData.propertyBlueprints) {
            dynamicState.propertyMap[blueprint.name] = new SmallObjectProperty(
                blueprint.name, blueprint.initialValue, blueprint.minValue, blueprint.maxValue);
        }
    }

    // 接收来自 IO 子系统的分发
    public void HandleInput(InputEventData eventData) {
        if (eventData.actionType == InputActionType.Interact) OnInteractAction(eventData);
        if (eventData.actionType == InputActionType.Movement) OnMoveAction(eventData.moveVector);
        if (eventData.actionType == InputActionType.ValueModify) OnValueModifyAction(eventData.propertyName, eventData.value);
    }

    // 子类实现具体逻辑
    public abstract void OnInteractAction(InputEventData eventData);
    public virtual void OnValueModifyAction(string prop, float delta) {
        if (dynamicState.propertyMap.ContainsKey(prop)) {
            if (NumericalRuleManager.TryModifyProperty(dynamicState.propertyMap[prop], delta)) {
                NotifyStateChange();
            }
        } else {
            Debug.LogWarning($"属性 {prop} 不存在于 {gameObject.name} 的属性映射中");
        }
    }
    public virtual void OnMoveAction(Vector3 moveVector) { }

    public void NotifyStateChange() {
        ownerBigObject?.OnChildStateChanged(this);
    }

    public virtual void ILabelOnAttacking(ILabelOwner target) { LabelOnAttacking?.Invoke(target); }
    public virtual void ILabelOnCrash(ILabelOwner Obstacle) { LabelOnCrash?.Invoke(Obstacle); }
    public virtual void ILabelOnAttacked(ILabelOwner attacker) { LabelOnAttacked?.Invoke(attacker); }
    public virtual void ILabelOnTick() { LabelOnTick?.Invoke(); }
    public virtual void ILabelOnMoving() { LabelOnMoving?.Invoke(); }

    // 尝试在有玩家在附近时激活显示标签（默认范围 2f，若是 NPC 使用 NPCStaticData.interactionRange）
    public virtual void OnActivateShowLabel() {
        var player = IOSubsystem.Instance?.playerObject;
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.transform.position);
        float range = 2f;
        if (dist <= range) {
            ActivateShowLabel();
        } else {
            Debug.Log("尝试显示标签但玩家不在范围内");
        }
    }

    // 实际激活事件（独立方法以便子类扩展）
    public virtual void ActivateShowLabel() {
        OnShowLabel?.Invoke(this);
    }

    // 尝试在有玩家在附近时激活显示属性（默认范围 2f）
    public virtual void OnActivateShowProperty() {
        var player = IOSubsystem.Instance?.playerObject;
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.transform.position);
        float range = 2f;
        if (dist <= range) {
            ActivateShowProperty();
        } else {
            Debug.Log("尝试显示属性但玩家不在范围内");
        }
    }

    // 实际激活属性显示事件（独立方法以便子类扩展）
    public virtual void ActivateShowProperty() {
        OnShowProperty?.Invoke(this);
    }

    // Provide methods to add/remove labels at runtime (store in dynamicState)
    public void AddLabel(ObjectLabel label) {
        if (label == null) return;
        if (dynamicState == null) dynamicState = CreateDynamicState();
        if (dynamicState.smallObjectLabels == null) dynamicState.smallObjectLabels = new System.Collections.Generic.List<ObjectLabel>();
        if (dynamicState.smallObjectLabels.Contains(label)) return;
        dynamicState.smallObjectLabels.Add(label);
        label.AttachToOwner(this);
    }

    public void RemoveLabel(ObjectLabel label) {
        if (label == null || dynamicState == null || dynamicState.smallObjectLabels == null) return;
        if (!dynamicState.smallObjectLabels.Contains(label)) return;
        dynamicState.smallObjectLabels.Remove(label);
        label.Detach();
    }

    public virtual void ReceiveDamage(DamagePacket packet) {
        Debug.Log($"{staticData.objectName} 收到了来自 {packet.attacker.staticData.objectName} 的 {packet.damageValue} 点伤害");

        // 1. 修改数值（利用我们之前的数值守恒系统）
        // 假设所有对象都有 "Health" 属性
        float damage = Mathf.Max(packet.damageValue * 0.05f, packet.damageValue - dynamicState.GetPropertyValueWithAffect("DEF")); // 伤害减去防御
        dynamicState.propertyMap["Health"].value -= damage;
        Debug.Log($"{staticData.objectName} 的当前生命值: {dynamicState.propertyMap["Health"].value}");
        // 2. 检查死亡
        if (dynamicState.propertyMap["Health"].value <= 0) {
            OnDeath();
            return; // 死亡后不执行击退
        }
        
        // 3. 执行击退（表现层逻辑）
        ApplyKnockback(packet.knockbackForce);
    }

    protected virtual void OnDeath() {
        dynamicState.isDestroyed = true;
        this.gameObject.SetActive(false);
    }

    protected virtual void ApplyKnockback(Vector2 force) {
        // 如果有 Rigidbody2D 则施加力
        dynamicState.isHurt = true;
        Debug.Log($"{staticData.objectName} 受到击退，力的大小: {force.magnitude}, 方向: {force.normalized}");
        Debug.Log($"玩家位置: {transform.position}, 速度: {GetComponent<Rigidbody2D>()?.linearVelocity}");
        GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Impulse);
        // 这里我们假设击退持续0.5秒，期间玩家无法控制
        StopCoroutine("RecoverFromKnockback");
        StartCoroutine(RecoverFromKnockback(0.25f)); // 0.25秒内不许动
    }

    private IEnumerator RecoverFromKnockback(float duration) {
        yield return new WaitForSeconds(duration);
        dynamicState.isHurt = false;
    }

    public virtual bool IsEnemy(SmallObject other) {
        return false;
    }
}