

[System.Serializable]
public interface ILabelOnAttacking { void ILabelOnAttacking(ILabelOwner target); }
public interface ILabelOnCrash { void ILabelOnCrash(ILabelOwner obstacle); }
public interface ILabelOnAttacked { void ILabelOnAttacked(ILabelOwner attacker); }
public interface ILabelOnTick { void ILabelOnTick(); }
public interface ILabelOnMoving { void ILabelOnMoving(); }

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
    public string labelName;

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
        owner = null;
    }
}