using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class LabelRuleTests {
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown() {
        for (int i = createdObjects.Count - 1; i >= 0; i--) {
            Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void ObjectLabelAttachToOwner_RegistersImplementedLabelCallbacksAndInvokesOnAttach() {
        RecordingLabelOwner owner = new RecordingLabelOwner();
        RecordingLabel label = new RecordingLabel();
        RecordingLabelOwner target = new RecordingLabelOwner();

        label.AttachToOwner(owner);

        owner.RaiseAttacking(target);
        owner.RaiseCrash(target);
        owner.RaiseAttacked(target);
        owner.RaiseTick();
        owner.RaiseMoving();

        Assert.That(label.owner, Is.SameAs(owner));
        Assert.That(label.AttachCallCount, Is.EqualTo(1));
        Assert.That(label.LastAttachedOwner, Is.SameAs(owner));
        Assert.That(label.AttackingCallCount, Is.EqualTo(1));
        Assert.That(label.CrashCallCount, Is.EqualTo(1));
        Assert.That(label.AttackedCallCount, Is.EqualTo(1));
        Assert.That(label.TickCallCount, Is.EqualTo(1));
        Assert.That(label.MovingCallCount, Is.EqualTo(1));
        Assert.That(label.LastAttackTarget, Is.SameAs(target));
        Assert.That(label.LastCrashObstacle, Is.SameAs(target));
        Assert.That(label.LastAttacker, Is.SameAs(target));
    }

    [Test]
    public void ObjectLabelDetach_UnregistersCallbacksClearsOwnerAndInvokesOnDetach() {
        RecordingLabelOwner owner = new RecordingLabelOwner();
        RecordingLabel label = new RecordingLabel();

        label.AttachToOwner(owner);
        label.Detach();

        owner.RaiseTick();
        owner.RaiseMoving();

        Assert.That(label.owner, Is.Null);
        Assert.That(label.DetachCallCount, Is.EqualTo(1));
        Assert.That(label.LastDetachedOwner, Is.SameAs(owner));
        Assert.That(label.TickCallCount, Is.EqualTo(0));
        Assert.That(label.MovingCallCount, Is.EqualTo(0));
    }

    [Test]
    public void ObjectLabelAttachToOwner_WhenMovedToNewOwner_DetachesFromPreviousOwner() {
        RecordingLabelOwner firstOwner = new RecordingLabelOwner();
        RecordingLabelOwner secondOwner = new RecordingLabelOwner();
        RecordingLabel label = new RecordingLabel();

        label.AttachToOwner(firstOwner);
        label.AttachToOwner(secondOwner);

        firstOwner.RaiseTick();
        secondOwner.RaiseTick();

        Assert.That(label.owner, Is.SameAs(secondOwner));
        Assert.That(label.DetachCallCount, Is.EqualTo(1));
        Assert.That(label.LastDetachedOwner, Is.SameAs(firstOwner));
        Assert.That(label.TickCallCount, Is.EqualTo(1));
    }

    [Test]
    public void ObjectLabelAttachToOwner_WhenOwnerIsNull_DoesNotAttach() {
        RecordingLabel label = new RecordingLabel();

        label.AttachToOwner(null);

        Assert.That(label.owner, Is.Null);
        Assert.That(label.AttachCallCount, Is.EqualTo(0));
    }

    [Test]
    public void SmallObjectAddLabel_StoresLabelAttachesItAndIgnoresDuplicateAdds() {
        DoorSmallObject smallObject = CreateSmallObject();
        RecordingLabel label = new RecordingLabel();

        smallObject.AddLabel(label);
        smallObject.AddLabel(label);
        smallObject.ILabelOnTick();

        Assert.That(smallObject.dynamicState.smallObjectLabels, Is.EquivalentTo(new[] { label }));
        Assert.That(label.owner, Is.SameAs(smallObject));
        Assert.That(label.AttachCallCount, Is.EqualTo(1));
        Assert.That(label.TickCallCount, Is.EqualTo(1));
    }

    [Test]
    public void SmallObjectRemoveLabel_RemovesLabelAndDetachesItFromEvents() {
        DoorSmallObject smallObject = CreateSmallObject();
        RecordingLabel label = new RecordingLabel();

        smallObject.AddLabel(label);
        smallObject.RemoveLabel(label);
        smallObject.ILabelOnTick();

        Assert.That(smallObject.dynamicState.smallObjectLabels, Has.No.Member(label));
        Assert.That(label.owner, Is.Null);
        Assert.That(label.DetachCallCount, Is.EqualTo(1));
        Assert.That(label.TickCallCount, Is.EqualTo(0));
    }

    [Test]
    public void BigObjectAddLabel_StoresLabelAttachesItAndIgnoresDuplicateAdds() {
        EnvironmentBigObject bigObject = CreateBigObject();
        RecordingLabel label = new RecordingLabel();

        bigObject.AddLabel(label);
        bigObject.AddLabel(label);
        bigObject.ILabelOnMoving();

        Assert.That(bigObject.dynamicState.bigObjectLabels, Is.EquivalentTo(new[] { label }));
        Assert.That(label.owner, Is.SameAs(bigObject));
        Assert.That(label.AttachCallCount, Is.EqualTo(1));
        Assert.That(label.MovingCallCount, Is.EqualTo(1));
    }

    [Test]
    public void BigObjectRemoveLabel_RemovesLabelAndDetachesItFromEvents() {
        EnvironmentBigObject bigObject = CreateBigObject();
        RecordingLabel label = new RecordingLabel();

        bigObject.AddLabel(label);
        bigObject.RemoveLabel(label);
        bigObject.ILabelOnMoving();

        Assert.That(bigObject.dynamicState.bigObjectLabels, Has.No.Member(label));
        Assert.That(label.owner, Is.Null);
        Assert.That(label.DetachCallCount, Is.EqualTo(1));
        Assert.That(label.MovingCallCount, Is.EqualTo(0));
    }

    [Test]
    public void AttachToSmallObject_UsesSmallObjectAsLabelOwner() {
        DoorSmallObject smallObject = CreateSmallObject();
        RecordingLabel label = new RecordingLabel();

        label.AttachToSmallObject(smallObject);
        smallObject.ILabelOnTick();

        Assert.That(label.owner, Is.SameAs(smallObject));
        Assert.That(label.AttachCallCount, Is.EqualTo(1));
        Assert.That(label.TickCallCount, Is.EqualTo(1));
    }

    private DoorSmallObject CreateSmallObject() {
        GameObject gameObject = new GameObject("SmallObject");
        gameObject.SetActive(false);
        createdObjects.Add(gameObject);

        DoorSmallObject smallObject = gameObject.AddComponent<DoorSmallObject>();
        smallObject.dynamicState = new SmallObjectDynamicState();
        return smallObject;
    }

    private EnvironmentBigObject CreateBigObject() {
        GameObject gameObject = new GameObject("BigObject");
        gameObject.SetActive(false);
        createdObjects.Add(gameObject);

        EnvironmentBigObject bigObject = gameObject.AddComponent<EnvironmentBigObject>();
        bigObject.dynamicState = new BigObjectDynamicState();
        return bigObject;
    }

    private sealed class RecordingLabelOwner : ILabelOwner {
        public event System.Action<ILabelOwner> LabelOnAttacking;
        public event System.Action<ILabelOwner> LabelOnCrash;
        public event System.Action<ILabelOwner> LabelOnAttacked;
        public event System.Action LabelOnTick;
        public event System.Action LabelOnMoving;

        public void RaiseAttacking(ILabelOwner target) {
            LabelOnAttacking?.Invoke(target);
        }

        public void RaiseCrash(ILabelOwner obstacle) {
            LabelOnCrash?.Invoke(obstacle);
        }

        public void RaiseAttacked(ILabelOwner attacker) {
            LabelOnAttacked?.Invoke(attacker);
        }

        public void RaiseTick() {
            LabelOnTick?.Invoke();
        }

        public void RaiseMoving() {
            LabelOnMoving?.Invoke();
        }
    }

    private sealed class RecordingLabel :
        ObjectLabel,
        ILabelOnAttacking,
        ILabelOnCrash,
        ILabelOnAttacked,
        ILabelOnTick,
        ILabelOnMoving {

        public int AttachCallCount;
        public int DetachCallCount;
        public int AttackingCallCount;
        public int CrashCallCount;
        public int AttackedCallCount;
        public int TickCallCount;
        public int MovingCallCount;
        public ILabelOwner LastAttachedOwner;
        public ILabelOwner LastDetachedOwner;
        public ILabelOwner LastAttackTarget;
        public ILabelOwner LastCrashObstacle;
        public ILabelOwner LastAttacker;

        public override void OnAttach(ILabelOwner owner) {
            AttachCallCount++;
            LastAttachedOwner = owner;
        }

        public override void OnDetach(ILabelOwner owner) {
            DetachCallCount++;
            LastDetachedOwner = owner;
        }

        public void ILabelOnAttacking(ILabelOwner target) {
            AttackingCallCount++;
            LastAttackTarget = target;
        }

        public void ILabelOnCrash(ILabelOwner obstacle) {
            CrashCallCount++;
            LastCrashObstacle = obstacle;
        }

        public void ILabelOnAttacked(ILabelOwner attacker) {
            AttackedCallCount++;
            LastAttacker = attacker;
        }

        public void ILabelOnTick() {
            TickCallCount++;
        }

        public void ILabelOnMoving() {
            MovingCallCount++;
        }
    }
}
