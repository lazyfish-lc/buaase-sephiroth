using UnityEngine;
using System.Collections.Generic;
public abstract class SmallObject : MonoBehaviour {
    public string objectName;
    public SmallObjectStaticData staticData;
    public SmallObjectDynamicState dynamicState;
    public BigObject ownerBigObject;
    // 包含对象所有的属性映射
    public Dictionary<string, SmallObjectProperty> propertyMap = new Dictionary<string, SmallObjectProperty>();

    protected virtual void Awake() {
        InitializeProperties();
    }

    public virtual void Start() {
        
    }

    private void InitializeProperties() {
        // 第一步：实例化所有定义的属性（确保后面规则引用时属性已存在）
        foreach (var blueprint in staticData.propertyBlueprints) {
            propertyMap[blueprint.name] = new SmallObjectProperty(blueprint.name, blueprint.initialValue);
        }
    }

    private void ApplyRule(NumericalRule rule) {
        // TODO: 加入规则到字典中
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
        if (propertyMap.ContainsKey(prop)) {
            propertyMap[prop].OnChanged(delta);
            NotifyStateChange();
        } else {
            Debug.LogWarning($"属性 {prop} 不存在于 {gameObject.name} 的属性映射中");
        }
    }
    public virtual void OnMoveAction(Vector3 moveVector) { }

    public void NotifyStateChange() {
        ownerBigObject?.OnChildStateChanged(this);
    }
}