using UnityEngine;

public class TreeSmallObject : SmallObject {
    public bool isDestroyed = false;

    public override void OnInteractAction() {
        if (isDestroyed) return;
        
        isDestroyed = true;
        Debug.Log("过去时空的树被砍倒了");
        this.gameObject.SetActive(false); // View 层的简单反馈
        NotifyStateChange();
    }
}
