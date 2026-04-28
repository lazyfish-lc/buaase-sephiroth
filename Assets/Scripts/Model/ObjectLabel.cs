using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface ILabelOnAttacking { void ILabelOnAttacking(ILabelOwner target); }
public interface ILabelOnCrash { void ILabelOnCrash(ILabelOwner obstacle); }
public interface ILabelOnAttacked { void ILabelOnAttacked(ILabelOwner attacker); }
public interface ILabelOnTick { void ILabelOnTick(); }
public interface ILabelOnMoving { void ILabelOnMoving(); }

// 新的 Label 接口：描述该 Label 会影响哪些属性，以及对单个属性的增量值
public interface ILabelAffectsProperty {
    // 返回该 Label 影响的属性名集合
    IEnumerable<string> GetAffectedPropertyNames();

    // 返回对指定属性的增量（可为负），如果不影响则返回 0
    float GetPropertyDelta(string propertyName);
}

// 统一的标签宿主接口，SmallObject 和 BigObject 都应实现此接口
public interface ILabelOwner {
    event System.Action<ILabelOwner> LabelOnAttacking;
    event System.Action<ILabelOwner> LabelOnCrash;
    event System.Action<ILabelOwner> LabelOnAttacked;
    event System.Action LabelOnTick;
    event System.Action LabelOnMoving;
}

[System.Serializable]
public class ObjectLabel {
    // 每个具体标签类的所有实例共享该名称（默认使用类名），
    // 保持与现有代码中通过实例访问 `labelName` 的方式兼容。
    public virtual string labelName {
        get {
            return this.GetType().Name;
        }
    }

    [System.NonSerialized]
    public ILabelOwner owner;

    public virtual void OnAttach(ILabelOwner owner) { }
    public virtual void OnDetach(ILabelOwner owner) { }

    // Attach this label to any ILabelOwner: register for events according to implemented interfaces
    public void AttachToOwner(ILabelOwner owner) {
        if (owner == null) return;
        // If already attached to another owner, detach first
        if (this.owner != null && this.owner != owner) {
            Detach();
        }

        this.owner = owner;

        if (this is ILabelOnAttacking attacking) owner.LabelOnAttacking += attacking.ILabelOnAttacking;
        if (this is ILabelOnCrash crash) owner.LabelOnCrash += crash.ILabelOnCrash;
        if (this is ILabelOnAttacked attacked) owner.LabelOnAttacked += attacked.ILabelOnAttacked;
        if (this is ILabelOnTick tick) owner.LabelOnTick += tick.ILabelOnTick;
        if (this is ILabelOnMoving moving) owner.LabelOnMoving += moving.ILabelOnMoving;

        OnAttach(owner);
    }

    // 保持兼容：挂载到 SmallObject 的便捷方法
    public void AttachToSmallObject(SmallObject owner) {
        AttachToOwner(owner);
    }

    // Detach this label from its owner: unregister event handlers
    public void Detach() {
        if (owner == null) return;

        if (this is ILabelOnAttacking attacking) owner.LabelOnAttacking -= attacking.ILabelOnAttacking;
        if (this is ILabelOnCrash crash) owner.LabelOnCrash -= crash.ILabelOnCrash;
        if (this is ILabelOnAttacked attacked) owner.LabelOnAttacked -= attacked.ILabelOnAttacked;
        if (this is ILabelOnTick tick) owner.LabelOnTick -= tick.ILabelOnTick;
        if (this is ILabelOnMoving moving) owner.LabelOnMoving -= moving.ILabelOnMoving;

        OnDetach(owner);
        // 如果原 owner 是 SmallObject，则从它的 dynamicState.smallObjectLabels 中移除自身
        var small = owner as global::SmallObject;
        if (small != null && small.dynamicState != null && small.dynamicState.smallObjectLabels != null) {
            small.dynamicState.smallObjectLabels.Remove(this);
        }

        owner = null;
    }
}