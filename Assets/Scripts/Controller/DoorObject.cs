using UnityEngine;

public class DoorSmallObject : SmallObject {
    public DoorObjectDynamicState doorState => (DoorObjectDynamicState) dynamicState;

    public override void OnInteractAction() {
        doorState.isOpen = !doorState.isOpen;
        Debug.Log("门状态: " + (doorState.isOpen ? "开启" : "关闭"));
        NotifyStateChange();
    }
    
    public void SetExistence(bool exists) {
        doorState.isDestroyed = !exists;
        this.gameObject.SetActive(exists);
    }
}