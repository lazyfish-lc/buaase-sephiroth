using UnityEngine;

public class TreeSmallObject : SmallObject {
    public TreeObjectDynamicState treeState => (TreeObjectDynamicState) dynamicState;

    public override void OnInteractAction() {
        if (treeState.isDestroyed) return;

        treeState.isDestroyed = true;
        Debug.Log("过去时空的树被砍倒了");
        this.gameObject.SetActive(false); // View 层的简单反馈
        NotifyStateChange();
    }
}
