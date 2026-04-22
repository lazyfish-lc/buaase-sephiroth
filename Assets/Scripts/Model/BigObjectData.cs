using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "BigObjectData", menuName = "Game/BigObjectData")]
public class BigObjectStaticData : ScriptableObject {
    public SmallObject pastObject;
    public SmallObject presentObject;
}

[Serializable]
public class BigObjectDynamicState {
    public List<ObjectLabel> bigObjectLabels = new List<ObjectLabel>();
}