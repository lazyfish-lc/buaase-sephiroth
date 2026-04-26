using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BigObject : MonoBehaviour, ILabelOwner {
    public BigObjectStaticData staticData;
    public BigObjectDynamicState dynamicState;

    // Label 事件（实现 ILabelOwner）
    public event Action<ILabelOwner> LabelOnAttacking;
    public event Action<ILabelOwner> LabelOnCrash;
    public event Action<ILabelOwner> LabelOnAttacked;
    public event Action LabelOnTick;
    public event Action LabelOnMoving;

    // 基本回调实现：触发事件，标签收到后可操作 owner（BigObject）并遍历子对象
    public virtual void ILabelOnAttacking(ILabelOwner target) { LabelOnAttacking?.Invoke(target); }
    public virtual void ILabelOnCrash(ILabelOwner obstacle) { LabelOnCrash?.Invoke(obstacle); }
    public virtual void ILabelOnAttacked(ILabelOwner attacker) { LabelOnAttacked?.Invoke(attacker); }
    public virtual void ILabelOnTick() { LabelOnTick?.Invoke(); }
    public virtual void ILabelOnMoving() { LabelOnMoving?.Invoke(); }

    // 核心逻辑：定义过去如何影响现在
    public abstract void OnChildStateChanged(SmallObject changedChild);

    public virtual void Start() {
        dynamicState = new BigObjectDynamicState();
        if(staticData.pastObject != null) staticData.pastObject.staticData.ownerBigObject = this;
        if(staticData.presentObject != null) staticData.presentObject.staticData.ownerBigObject = this;
        GameObjectManager.RegisterBigObject(this);

        // 挂载已经配置在 dynamicState 中的 labels
        if (dynamicState != null && dynamicState.bigObjectLabels != null) {
            foreach (var label in dynamicState.bigObjectLabels) label?.AttachToOwner(this);
        }
    }

    public virtual void OnDestroy() {
        // 取消挂载 labels
        if (dynamicState != null && dynamicState.bigObjectLabels != null) {
            foreach (var label in dynamicState.bigObjectLabels) label?.Detach();
        }
        GameObjectManager.UnregisterBigObject(this);
    }

    public void AddLabel(ObjectLabel label) {
        if (label == null) return;
        if (dynamicState == null) dynamicState = new BigObjectDynamicState();
        if (dynamicState.bigObjectLabels == null) dynamicState.bigObjectLabels = new System.Collections.Generic.List<ObjectLabel>();
        if (dynamicState.bigObjectLabels.Contains(label)) return;
        dynamicState.bigObjectLabels.Add(label);
        label.AttachToOwner(this);
    }

    public void RemoveLabel(ObjectLabel label) {
        if (label == null || dynamicState == null || dynamicState.bigObjectLabels == null) return;
        if (!dynamicState.bigObjectLabels.Contains(label)) return;
        dynamicState.bigObjectLabels.Remove(label);
        label.Detach();
    }
}