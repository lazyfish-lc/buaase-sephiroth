using UnityEngine;

public class DoorSmallObject : SmallObject {
    public bool isOpen = false;
    public bool isDestroyed = false; // 额外状态，表示门是否被破坏（例如被树的状态影响）

    public override void OnInteractAction() {
        isOpen = !isOpen;
        Debug.Log("门状态: " + (isOpen ? "开启" : "关闭"));
        NotifyStateChange();
    }
    
    public void SetExistence(bool exists) {
        isDestroyed = !exists;
        this.gameObject.SetActive(exists);
    }
}