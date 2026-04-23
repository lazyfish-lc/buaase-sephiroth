using UnityEngine;
using System.Collections.Generic;
public abstract class SmallObject : MonoBehaviour {
    
    public SmallObjectStaticData staticData;
    public SmallObjectDynamicState dynamicState;
    
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
}