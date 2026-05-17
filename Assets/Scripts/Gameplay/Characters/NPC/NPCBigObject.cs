using UnityEngine;

public class NPCBigObject : BigObject {
    public override void OnChildStateChanged(SmallObject changedChild) {
        // 这里可以根据需要实现 NPC 对子对象状态变化的反应逻辑
        // 例如，如果某个子对象被破坏了，NPC 可能会改变行为或状态
    }
}