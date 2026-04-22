using System;
using UnityEngine;

[Serializable]
public class DoorObjectDynamicState : SmallObjectDynamicState {
    public bool isOpen = false;
    public bool isDestroyed = false; // 额外状态，表示门是否被破坏（例如被树的状态影响）
}