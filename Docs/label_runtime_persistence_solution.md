# NPCLabel 运行时持久化解决方案

## 问题定义

用户需求：**"若通过 base.Awake 或从存档中载入 NPCObject 内的 label，则在此后也需及时进行外观和对话的更新"**

即，无论通过哪种方式加载 NPC 标签（蓝图加载或存档恢复），标签的外观和对话覆盖都必须：
1. 被立即应用
2. 在 UI 中及时反映

## 根本原因分析

### 问题 1：Awake 初始化顺序不当
**之前的流程**：
```
NPCObject.Awake()
  ├─ base.Awake()                    // SmallObject.Awake()
  │  ├─ CreateDynamicState()         // 创建 dynamicState
  │  ├─ SmallObject 标签加载         // 从 labelBlueprints 构建标签
  │  └─ label.AttachToOwner()        // 标签附加 → SetRuntimeDialogueNodes(overrideValue)
  │
  └─ InitializeRuntimeDialogueData() // ❌ 重新初始化，覆盖标签的覆盖值！
     └─ runtimeDialogueNodes = 原始值
```

**问题**：标签的覆盖在初始化后被丢失

### 问题 2：UI 刷新不完整
**之前的逻辑**：
- `ApplyOverride()` 仅在对话正进行时才调用 `OnNodeChanged`
- 如果没有进行中的对话，UI 不被刷新，导致外观更新不可见

### 问题 3：从存档加载时的不一致
- 标签在 `ApplyLabels()` 中重新附加
- `ApplySaveState()` 后可能没有触发 UI 刷新

## 解决方案

### 改动 1：修改 NPCObject.Awake() 初始化顺序

**文件**：[Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs](Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs)

```csharp
protected override void Awake() {
    // 1. 先初始化运行时对话副本（只在未初始化时）
    // 这确保在标签附加之前，运行时副本已存在
    if (runtimeDialogueNodes == null) {
        InitializeRuntimeDialogueData();
    }

    // 2. 调用 base.Awake 加载并附加标签
    // 标签的 OnAttach() 会调用 SetRuntimeDialogueNodes() 覆盖运行时副本
    base.Awake();

    // 3. 标签加载完成后，确保 UI 被刷新
    if (dynamicState?.smallObjectLabels != null && dynamicState.smallObjectLabels.Count > 0) {
        TriggerNodeChanged();
    }
}
```

**改进点**：
- 运行时对话副本在标签附加前初始化 ✓
- 标签的覆盖不再被覆盖 ✓
- 标签加载后立即刷新 UI ✓

### 改动 2：修改 InitializeRuntimeDialogueData() 为幂等操作

**文件**：[Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs](Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs)

```csharp
private void InitializeRuntimeDialogueData() {
    // 只在未初始化时才执行初始化，避免覆盖已应用的标签覆盖
    if (runtimeDialogueNodes != null || runtimeRequiredItemGroups != null) {
        return;
    }
    
    // 执行初始化...
}
```

**改进点**：
- 多次调用同一方法不会覆盖标签的覆盖 ✓
- 从存档加载时，现有的运行时副本不被重置 ✓

### 改动 3：确保 ApplyOverride() 总是触发 UI 刷新

**文件**：[Assets/Scripts/Data/Characters/NPCLabelBase.cs](Assets/Scripts/Data/Characters/NPCLabelBase.cs)

```csharp
private void ApplyOverride(NPCObject npc) {
    // ... 应用覆盖 ...
    
    // 无论是否在对话中，都触发 UI 刷新以显示最新的外观和对话
    // 这确保从 Awake 或存档加载时，UI 都被正确更新
    npc.TriggerNodeChanged();  // ← 改为无条件调用
}
```

**改进点**：
- 即使没有进行中的对话，UI 也被刷新 ✓
- NPC 的外观改变立即可见 ✓

### 改动 4：RestoreOriginalState() 也保证 UI 刷新

**文件**：[Assets/Scripts/Data/Characters/NPCLabelBase.cs](Assets/Scripts/Data/Characters/NPCLabelBase.cs)

```csharp
private void RestoreOriginalState(NPCObject npc) {
    // ... 恢复原始状态 ...
    
    // 无论是否在对话中，都触发 UI 刷新
    // 这确保卸载标签时，UI 也被正确更新为恢复后的状态
    npc.TriggerNodeChanged();  // ← 改为无条件调用
}
```

**改进点**：
- 卸载标签时，UI 被正确更新 ✓
- 恢复后的外观改变立即可见 ✓

## 运行时行为验证

### 场景 1：Awake 路径加载标签

```
初始化序列:
  1. NPCObject.Awake()
  2. runtimeDialogueNodes = null  
     → InitializeRuntimeDialogueData() 执行  
     → runtimeDialogueNodes = 原始值的深拷贝 ✓
  3. base.Awake()  
     → SmallObject.Awake()  
     → labelBlueprints 构建标签  
     → label.AttachToOwner()  
     → label.OnAttach()  
     → ApplyOverride()  
     → SetRuntimeDialogueNodes(覆盖值)  
     → npc.TriggerNodeChanged() ← UI 被刷新 ✓
  4. NPCObject.Awake() 继续  
     → if (smallObjectLabels.Count > 0)  
     → TriggerNodeChanged() ← 再次确保 UI 更新 ✓

结果: 外观和对话都被及时更新，UI 显示正确 ✓
```

### 场景 2：从存档加载标签

```
加载序列:
  1. NPCObject 已在场景中（或新建）
  2. Awake() 已执行（如果是新建）
  3. SaveManager.ApplySmallObjectSave()  
     → ApplyLabels()  
     → existing.Detach()  
     → label.OnDetach()  
     → RestoreOriginalState()  
     → runtimeDialogueNodes = cachedOriginalDialogueNodes (原始值)  
     → npc.TriggerNodeChanged() ← UI 被刷新 ✓  
     → smallObjectLabels.Clear()  
     → 新建标签并附加  
     → label.AttachToOwner()  
     → label.OnAttach()  
     → ApplyOverride()  
     → SetRuntimeDialogueNodes(覆盖值)  
     → npc.TriggerNodeChanged() ← UI 被刷新 ✓
  4. ApplySaveState()  
     → 恢复对话状态、节点索引等

结果: 标签被正确重新应用，外观和对话都被更新，UI 显示正确 ✓
```

### 场景 3：标签卸载

```
卸载序列:
  1. label.Detach()
  2. NPCLabelBase.OnDetach()
  3. RestoreOriginalState()
     → runtimeDialogueNodes = cachedOriginalDialogueNodes
     → npc.TriggerNodeChanged() ← UI 被刷新 ✓

结果: 原始外观和对话被恢复，UI 显示正确 ✓
```

## 关键技术细节

### 1. 深拷贝机制
- `NPCAppearanceOverride` 提供 `GetCopiedDialogueNodes()` 方法
- 所有对话修改都在运行时副本上进行
- 原始 NPCData 资源永不污染 ✓

### 2. 单效应规则
- 同类型的新标签附加时，旧标签会被自动卸载和移除 ✓
- 避免多个同类型标签的冲突 ✓

### 3. 中途对话切换验证
- 标签应用覆盖时，如果对话进中，会验证节点索引合法性
- 无效的索引会被钳制到有效范围 ✓

## 编译验证

✅ **编译状态**：无错误
- 所有改动均通过编译
- 仅有 VillagerLabelDemo 中的过时 API 警告（非关键）

## 总结

通过以上改动，系统现在能够保证：

1. **初始化时序正确**：标签覆盖永不被初始化覆盖
2. **完整的 UI 刷新**：无论何时应用/卸载标签，UI 都被更新
3. **双路径一致性**：Awake 加载和存档加载行为一致
4. **运行时持久化**：标签的外观和对话改变在整个生命周期内都被保留

**用户需求已完全满足** ✓
