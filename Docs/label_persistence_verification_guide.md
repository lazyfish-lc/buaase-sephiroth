# NPCLabel 运行时持久化 - 完整验证指南

## 概述

本指南描述如何验证 NPCLabel 系统在所有加载路径中都正确保持外观和对话更新。

## 验证清单

### ✓ 改动 1：初始化顺序修正
- **文件**：`Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs`
- **验证方法**：
  1. 检查 `Awake()` 中 `InitializeRuntimeDialogueData()` 在 `base.Awake()` 之前调用
  2. 确认 `InitializeRuntimeDialogueData()` 有guard条件（`if (runtimeDialogueNodes == null)`）
  3. 标志：只初始化一次，不会覆盖标签覆盖

### ✓ 改动 2：UI 刷新保证
- **文件**：`Assets/Scripts/Data/Characters/NPCLabelBase.cs`
- **验证方法**：
  1. `ApplyOverride()` 末尾总是调用 `npc.TriggerNodeChanged()`
  2. `RestoreOriginalState()` 末尾也总是调用 `npc.TriggerNodeChanged()`
  3. 标志：无条件刷新，外观改变立即可见

### ✓ 改动 3：Awake 路径完整性
- **文件**：`Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs`
- **验证方法**：
  1. `Awake()` 末尾检查是否存在标签
  2. 如果存在标签，调用 `TriggerNodeChanged()` 二次刷新
  3. 标志：确保标签加载后 UI 完全同步

### ✓ 改动 4：编译验证
- **验证方法**：
  1. 打开 Unity 并让项目编译
  2. 检查 Console 中是否有编译错误（警告可以忽略）
  3. 标志：无编译错误

## 测试场景

### 场景 A：Awake 加载标签验证

**步骤**：
1. 打开包含 NPCObject 的场景
2. 在 NPCObject Inspector 中设置标签蓝图为 VillagerLabel
3. 运行游戏，立即检查 NPC 外观
4. 预期：NPC 外观应立即显示 VillagerLabel 的覆盖（如果设置了 override）

**验证点**：
```
预期行为序列:
  1. NPCObject.Awake() 执行
  2. runtimeDialogueNodes 初始化为原始值
  3. base.Awake() 加载标签
  4. VillagerLabel.OnAttach() 执行 → SetRuntimeDialogueNodes(override)
  5. UI 刷新（通过 ApplyOverride → TriggerNodeChanged）
  
观察结果: NPC 显示覆盖外观，UI 一致无闪烁
```

**测试代码**：
- 将 `NPCLabelPersistenceTest.cs` 挂载到场景中
- 按 K：测试手动加载标签
- 按 L：测试卸载标签
- 按 U：打印运行时状态日志
- 按 V：显示当前外观和对话

### 场景 B：存档加载标签验证

**步骤**：
1. 在运行游戏中与 NPC 交互，确保 NPC 有标签覆盖
2. 保存游戏（Ctrl+S 或游戏菜单）
3. 停止游戏
4. 重新运行游戏并加载存档
5. 检查 NPC 外观是否与保存前相同

**验证点**：
```
预期行为序列:
  1. 场景加载，NPCObject.Awake() 执行
  2. SaveManager 处理恢复
  3. ApplyLabels() 被调用，标签重新附加
  4. VillagerLabel.OnAttach() → SetRuntimeDialogueNodes(override)
  5. UI 刷新（通过 ApplyOverride → TriggerNodeChanged）
  6. ApplySaveState() 恢复对话状态
  
观察结果: NPC 显示保存前的覆盖外观，对话状态正确恢复
```

### 场景 C：标签卸载验证

**步骤**：
1. 在游戏中应用标签（K 键或从蓝图附加）
2. 按 L 卸载标签
3. 检查 NPC 外观是否恢复为原始状态

**验证点**：
```
预期行为序列:
  1. label.Detach() 被调用
  2. RestoreOriginalState() 执行
  3. runtimeDialogueNodes 恢复为 cachedOriginalDialogueNodes
  4. UI 刷新（通过 RestoreOriginalState → TriggerNodeChanged）
  
观察结果: NPC 外观立即恢复，无闪烁
```

### 场景 D：跨实例一致性验证

**步骤**：
1. 在多个 NPC 身上应用标签
2. 检查每个 NPC 是否都正确显示其标签覆盖
3. 保存并重新加载

**验证点**：
- 每个 NPC 都维持正确的标签状态
- 标签之间无冲突干扰

## 自动化测试代码

已在 `Assets/Scripts/Tests/NPCLabelPersistenceTest.cs` 中实现以下测试：

```csharp
// 键盘控制：
K - AttachVillagerLabel()   // 手动附加标签
L - DetachVillagerLabel()   // 手动卸载标签
U - VerifyRuntimeState()    // 打印运行时状态
V - DisplayAppearanceAndDialogue() // 显示当前外观和对话
```

**输出示例**：
```
[TEST] VillagerLabel attached via manual code
=== NPCLabel Runtime State Verification ===
Original NPCData Dialogue Nodes: 3
  First node content (original): "Hello, traveler!"
Runtime Dialogue Nodes: 3
  First node content (runtime): "Well met, friend!"
Attached Labels: 1
  - VillagerLabel
Current Sprite: VillagerSprite
========================================
```

## 预期结果

### ✅ 成功指标

| 场景 | 预期结果 | 验证方式 |
|------|---------|---------|
| Awake 加载 | 标签覆盖立即应用 | UI 显示正确，无延迟 |
| 存档加载 | 标签恢复后立即刷新 | 恢复后外观一致 |
| 标签卸载 | 原始状态立即恢复 | UI 切换无闪烁 |
| 日志验证 | 运行时副本正确 | U 键日志显示一致 |

### ❌ 失败指标

| 问题征象 | 可能原因 | 修复 |
|---------|---------|------|
| 标签加载后外观未变化 | ApplyOverride 不调用 OnNodeChanged | 检查改动 3 |
| 存档加载外观闪烁 | 初始化顺序错误 | 检查改动 1 |
| 运行时副本被覆盖 | InitializeRuntimeDialogueData 缺少 guard | 检查改动 2 |
| UI 多次刷新 | TriggerNodeChanged 调用过多 | 简化调用链 |

## 调试分步指南

### 问题：标签加载后外观未改变

1. **检查 ApplyOverride 是否被调用**：
   ```csharp
   // 在 NPCLabelBase.ApplyOverride 中添加日志
   Debug.Log($"[NPCLabelBase] Applying override to {npc.name}");
   ```

2. **检查 TriggerNodeChanged 是否被调用**：
   ```csharp
   // 在末尾检查
   Debug.Log($"[NPCLabelBase] Calling TriggerNodeChanged");
   npc.TriggerNodeChanged();
   ```

3. **检查回调是否被订阅**：
   ```csharp
   // 检查 OnNodeChanged 事件是否有订阅者
   if (OnNodeChanged == null) Debug.LogWarning("OnNodeChanged has no subscribers!");
   ```

### 问题：存档加载后标签不应用

1. **检查 SaveManager.ApplyLabels 是否被调用**：
   ```csharp
   Debug.Log($"[SaveManager] Applying {labels.Count} labels to {target.name}");
   ```

2. **检查标签构建是否成功**：
   ```csharp
   var label = BuildLabel(labelData);
   if (label == null) Debug.LogWarning($"Failed to build label: {labelData.labelType}");
   ```

3. **检查资源加载是否成功**：
   ```csharp
   var config = Resources.Load<NPCAppearanceOverride>($"NPCAppearanceOverrides/{data.npcOverrideConfigName}");
   if (config == null) Debug.LogWarning($"Failed to load override config: {data.npcOverrideConfigName}");
   ```

## 代码审查清单

- [ ] `NPCObject.Awake()` 中 `InitializeRuntimeDialogueData()` 在 `base.Awake()` 之前
- [ ] `InitializeRuntimeDialogueData()` 有 guard 条件
- [ ] `ApplyOverride()` 末尾无条件调用 `TriggerNodeChanged()`
- [ ] `RestoreOriginalState()` 末尾无条件调用 `TriggerNodeChanged()`
- [ ] `Awake()` 末尾检查标签计数后调用 `TriggerNodeChanged()`
- [ ] 编译无错误
- [ ] 所有改动已保存

## 总结

通过以上验证，可以确保：
1. ✅ 标签在 Awake 中立即应用，UI 同步更新
2. ✅ 标签从存档加载后正确恢复，UI 同步更新
3. ✅ 标签卸载时，原始状态正确恢复，UI 同步更新
4. ✅ 整个生命周期中，运行时副本和 UI 始终保持一致

**所有验证通过 = 用户需求已满足** ✓
