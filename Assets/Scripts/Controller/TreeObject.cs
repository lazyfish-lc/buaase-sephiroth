using UnityEngine;

public class TreeSmallObject : SmallObject {
    public TreeObjectDynamicState treeState => dynamicState as TreeObjectDynamicState;

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new TreeObjectDynamicState();
    }

    public override void OnInteractAction() {
        if (treeState == null) {
            Debug.LogWarning("树状态类型配置错误，无法交互");
            return;
        }

        if (treeState.isDestroyed) return;

        treeState.isDestroyed = true;
        Debug.Log("过去时空的树被砍倒了");
        this.gameObject.SetActive(false); // View 层的简单反馈
        NotifyStateChange();
    }
}
