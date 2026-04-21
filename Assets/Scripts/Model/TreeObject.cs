using UnityEngine;

public class TreeSmallObject : SmallObject {
    public override void OnInteractAction() {
        if (dynamicState.isDestroyed) return;
        
        dynamicState.isDestroyed = true;
        Debug.Log("过去时空的树被砍倒了");
        this.gameObject.SetActive(false); // View 层的简单反馈
        NotifyStateChange();
    }
}
