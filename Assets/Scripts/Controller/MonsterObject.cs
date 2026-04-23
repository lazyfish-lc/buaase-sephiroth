using UnityEngine;

public class MonsterSmallObject : SmallObject {
    public MonsterObjectDynamicState monsterState => dynamicState as MonsterObjectDynamicState;

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new MonsterObjectDynamicState();
    }
    
    public override void OnValueModifyAction(string prop, float delta) {
        if (monsterState == null) {
            Debug.LogWarning("怪物状态类型配置错误，无法修改数值");
            return;
        }

        // 修改血量
        monsterState.currentHealth += delta * 50f;
        Debug.Log($"怪物 {staticData.objectName} 血量变更为: {monsterState.currentHealth}");
        
        // 此处可触发【核心玩法1：数值守恒】
        // ValueManager.Instance.ApplyConservation(this, delta);
    }

    public override void OnInteractAction() {
        Debug.Log("对怪物进行交互（如：偷窃标签）");
    }
}