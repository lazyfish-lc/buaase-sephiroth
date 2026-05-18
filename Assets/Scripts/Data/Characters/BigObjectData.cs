using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "BigObjectData", menuName = "Game/BigObjectData")]
public class BigObjectStaticData : ScriptableObject {
    // 在资源中记录该 BigObject 初始化时需挂载的标签描述（通过 LabelFactory 创建）
    public List<string> labelBlueprints = new List<string>();
}

[Serializable]
public class BigObjectDynamicState {
    public List<ObjectLabel> bigObjectLabels = new List<ObjectLabel>();
}