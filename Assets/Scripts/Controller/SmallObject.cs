using UnityEngine;
using System.Collections.Generic;
using System.Collections;         // 必须有这个，用于 IEnumerator
public abstract class SmallObject : MonoBehaviour {
    
    public SmallObjectStaticData staticData;
    public SmallObjectDynamicState dynamicState;
    public bool isHurt = false; // 是否处于受击状态，受击状态下可能无法移动或攻击
    
    // 包含对象所有的属性映射
    

    protected virtual void Awake() {
        dynamicState = CreateDynamicState();
        if (dynamicState == null) {
            dynamicState = new SmallObjectDynamicState();
            Debug.LogWarning($"{GetType().Name} CreateDynamicState 返回空，已回退为 SmallObjectDynamicState");
        }
        InitializeProperties();
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
        if (eventData.actionType == InputActionType.Interact) OnInteractAction();
        if (eventData.actionType == InputActionType.Movement) OnMoveAction(eventData.moveVector);
        if (eventData.actionType == InputActionType.ValueModify) OnValueModifyAction(eventData.propertyName, eventData.value);
    }

    // 子类实现具体逻辑
    public abstract void OnInteractAction();
    public virtual void OnValueModifyAction(string prop, float delta) {
        if (dynamicState.propertyMap.ContainsKey(prop)) {
            dynamicState.propertyMap[prop].SetValue(delta);
            NotifyStateChange();
        } else {
            Debug.LogWarning($"属性 {prop} 不存在于 {gameObject.name} 的属性映射中");
        }
    }
    public virtual void OnMoveAction(Vector3 moveVector) { }

    public void NotifyStateChange() {
        staticData.ownerBigObject?.OnChildStateChanged(this);
    }

    public virtual void ILabelOnAttacking(SmallObject target) { }
    public virtual void ILabelOnCrash(SmallObject Obstacle) { }
    public virtual void ILabelOnAttacked(SmallObject attacker) { }
    public virtual void ILabelOnTick() { }
    public virtual void ILabelOnMoving() { }


    public virtual void ReceiveDamage(DamagePacket packet) {
        Debug.Log($"{staticData.objectName} 收到了来自 {packet.attacker.staticData.objectName} 的 {packet.damageValue} 点伤害");

        // 1. 修改数值（利用我们之前的数值守恒系统）
        // 假设所有对象都有 "Health" 属性
        dynamicState.propertyMap["Health"].value -= (packet.damageValue - dynamicState.propertyMap["DEF"].value);
        Debug.Log($"{staticData.objectName} 的当前生命值: {dynamicState.propertyMap["Health"].value}");
        // 2. 检查死亡
        if (dynamicState.propertyMap["Health"].value <= 0) {
            OnDeath();
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
        isHurt = true;
        Debug.Log($"{staticData.objectName} 受到击退，力的大小: {force.magnitude}, 方向: {force.normalized}");
        Debug.Log($"玩家位置: {transform.position}, 速度: {GetComponent<Rigidbody2D>()?.linearVelocity}");
        GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Impulse);
        // 这里我们假设击退持续0.5秒，期间玩家无法控制
        StopCoroutine("RecoverFromKnockback");
        StartCoroutine(RecoverFromKnockback(0.25f)); // 0.25秒内不许动
    }

    private IEnumerator RecoverFromKnockback(float duration) {
        yield return new WaitForSeconds(duration);
        isHurt = false;
    }
}