using UnityEngine;

public class DoorSmallObject : SmallObject {
    public DoorObjectDynamicState doorState => dynamicState as DoorObjectDynamicState;

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new DoorObjectDynamicState();
    }

    public override void OnInteractAction(InputEventData data) {
        if (doorState == null) {
            Debug.LogWarning("门状态类型配置错误，无法交互");
            return;
        }

        doorState.isOpen = !doorState.isOpen;
        Debug.Log("门状态: " + (doorState.isOpen ? "开启" : "关闭"));
        NotifyStateChange();
    }
    
    public void SetExistence(bool exists) {
        if (doorState == null) return;
        doorState.isDestroyed = !exists;
        this.gameObject.SetActive(exists);
    }
}