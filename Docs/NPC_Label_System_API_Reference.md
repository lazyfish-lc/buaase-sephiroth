# NPC 标签系统 - API 快速参考

## NPCAppearanceOverride

```csharp
// 创建 (编辑器中右键 Create > Game > NPCAppearanceOverride)
NPCAppearanceOverride config = AssetDatabase.LoadAssetAtPath<NPCAppearanceOverride>(path);

// 配置字段
config.overrideSprite;              // Sprite
config.overrideAnimatorController;  // RuntimeAnimatorController
config.overrideDialogueNodes;       // List<DialogueNode>
config.overrideRequiredItemGroups;  // List<NPCStaticData.RequiredItemGroup>

// 获取深拷贝
List<DialogueNode> copied = config.GetCopiedDialogueNodes();
List<NPCStaticData.RequiredItemGroup> copied = config.GetCopiedRequiredItemGroups();
```

## NPCLabelBase

```csharp
// 创建标签实例
var label = new VillagerLabel();
var label = new VillagerLabel(config);

// 生命周期
label.OnAttach(npc);    // 自动缓存原始状态、应用覆盖
label.OnDetach(npc);    // 自动恢复原始状态

// 属性
label.labelName;        // 标签名称 (如 "Villager")
label.owner;            // 绑定的 ILabelOwner (如 NPCObject)
```

## NPCObject

### 挂载/卸载标签
```csharp
npc.AddLabel(label);        // 挂载标签
npc.RemoveLabel(label);     // 卸载标签
```

### 对话接口 (现在读运行时副本)
```csharp
string content = npc.GetCurrentContent();           // 当前对话内容
Sprite sprite = npc.GetCurrentSprite();             // 当前 Sprite
List<DialogueOption> options = npc.GetCurrentOptions(); // 当前选项
bool hasOptions = npc.CurrentNodeHasOptions();      // 是否有选项
int nodeIndex = npc.GetCurrentNodeIndex();          // 当前节点索引
bool inConv = npc.IsInConversation();               // 是否在对话中
```

### 标签使用专用接口
```csharp
npc.GetNPCData();  // 获取原始 NPCData (只读)

// 设置运行时副本 (仅在标签中调用)
npc.SetRuntimeDialogueNodes(List<DialogueNode>);
npc.SetRuntimeRequiredItemGroups(List<NPCStaticData.RequiredItemGroup>);
npc.SetCurrentNodeIndex(int);
npc.TriggerNodeChanged();  // 触发 UI 刷新
```

### 其他
```csharp
npc.ApplySaveState(bool isInConversation, int nodeIndex, PlayerSmallObject player);
```

## 标签生命周期

```
┌──────────────────┐
│ new VillagerLabel│
│  (config)        │
└────────┬─────────┘
         ↓
    npc.AddLabel(label)
         ↓
    ┌─────────────────────┐
    │ label.OnAttach()    │ ← 此时执行
    │ • 缓存原始状态      │
    │ • 应用覆盖          │
    │ • 刷新 UI (如需)    │
    └─────────────────────┘
         ↓
    [标签生效期间]
         ↓
    npc.RemoveLabel(label)
         ↓
    ┌─────────────────────┐
    │ label.OnDetach()    │ ← 此时执行
    │ • 恢复原始状态      │
    │ • 刷新 UI (如需)    │
    │ • 清空缓存          │
    └─────────────────────┘
         ↓
    [标签移除]
```

## 常见用法

### 简单使用
```csharp
// 创建标签并挂载
var config = Resources.Load<NPCAppearanceOverride>("NPCAppearanceOverrides/MyConfig");
var label = new VillagerLabel(config);
NPCObject.AddLabel(label);

// 卸载标签
npc.RemoveLabel(label);
```

### 在编辑器中配置
```
1. 在 NPCStaticData 的 labelBlueprints 添加: VillagerLabel
2. NPCObject 初始化时自动创建并挂载该标签
```

### 存档支持
```csharp
// 存档时自动保存标签
SaveManager.Instance.SaveGame();

// 读档时自动恢复标签
SaveManager.Instance.LoadGame();
```

### 对话中切换标签
```csharp
// 标签会自动验证节点索引合法性
if (npc.IsInConversation()) {
    npc.AddLabel(newLabel);  // 自动刷新 UI
}
```

## 创建自定义标签

```csharp
[Serializable]
public class MyCustomLabel : NPCLabelBase {
    
    public MyCustomLabel() { }
    
    public MyCustomLabel(NPCAppearanceOverride config) : base(config) { }
    
    public override string labelName => "MyCustom";
    
    // 可选：重写生命周期方法
    public override void OnAttach(ILabelOwner owner) {
        base.OnAttach(owner);
        // 自定义初始化
    }
    
    public override void OnDetach(ILabelOwner owner) {
        base.OnDetach(owner);
        // 自定义清理
    }
}
```

## 调试技巧

```csharp
// 查询当前标签列表
if (npc.dynamicState?.smallObjectLabels != null) {
    foreach (var label in npc.dynamicState.smallObjectLabels) {
        Debug.Log($"标签: {label.labelName}");
    }
}

// 检查当前对话数据源
// GetCurrentDialogueNodes() 返回的是运行时副本
// 原始数据保存在 npc.GetNPCData().dialogueNodes

// 验证节点索引
int currentIndex = npc.GetCurrentNodeIndex();
int maxIndex = npc.GetCurrentDialogueNodes().Count - 1;
if (currentIndex > maxIndex) {
    Debug.LogWarning("节点索引越界!");
}
```

## 性能提示

- ✓ 深拷贝仅在标签挂载时执行一次
- ✓ 对话查询使用运行时副本，不会每次都遍历列表
- ✓ UI 刷新仅在必要时触发（节点切换时）
- ✓ 标签卸载时及时清空缓存

## 常见问题

### Q: 为什么我的标签没有生效?
A: 检查:
1. NPCAppearanceOverride 资产是否正确配置
2. 标签是否正确挂载 (npc.AddLabel)
3. 标签的 overrideConfig 是否为 null

### Q: 为什么对话没有更新?
A: 检查:
1. 标签的覆盖对话列表是否配置正确
2. UI 是否正确订阅了 OnNodeChanged 事件
3. 节点索引是否合法

### Q: 如何同时应用多个标签?
A: 
1. 挂载不同类型的标签 (如 VillagerLabel + GlowLabel)
2. 同类型标签不支持堆叠，新标签会卸载旧标签

### Q: 标签状态如何保存?
A: 
1. SaveManager 自动保存标签列表
2. 读档时根据保存的配置名称加载资产
3. 确保 NPCAppearanceOverride 资产在 `Resources/NPCAppearanceOverrides/` 路径

---

**快速参考单** | NPC 标签系统 v1.0
