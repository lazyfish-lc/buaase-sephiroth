using UnityEngine;

public class MonsterSmallObject : SmallObject {
    public MonsterObjectDynamicState monsterState => (MonsterObjectDynamicState) dynamicState;
    
    public override void OnValueModifyAction(string prop, float delta) {
        // 修改血量
        dynamicState.currentHealth += delta * 50f;
        Debug.Log($"怪物 {staticData.objectName} 血量变更为: {dynamicState.currentHealth}");
        
        // 此处可触发【核心玩法1：数值守恒】
        // ValueManager.Instance.ApplyConservation(this, delta);
    }

    public override void OnInteractAction() {
        Debug.Log("对怪物进行交互（如：偷窃标签）");
    }
}