using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DoorObjectData", menuName = "Game/DoorStaticData")]
public class DoorStaticData : SmallObjectStaticData {
    public bool initiallyOpen = false; // 门初始状态
}

[Serializable]
public class DoorObjectDynamicState : SmallObjectDynamicState {
    public bool isOpen = false; // 额外状态，表示门是否被破坏（例如被树的状态影响）
}