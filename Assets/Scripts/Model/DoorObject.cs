using UnityEngine;

public class DoorSmallObject : SmallObject {
    public override void OnInteractAction() {
        dynamicState.boolState = !dynamicState.boolState;
        Debug.Log("门状态: " + (dynamicState.boolState ? "开启" : "关闭"));
        NotifyStateChange();
    }
    
    public void SetExistence(bool exists) {
        dynamicState.isDestroyed = !exists;
        this.gameObject.SetActive(exists);
    }
}