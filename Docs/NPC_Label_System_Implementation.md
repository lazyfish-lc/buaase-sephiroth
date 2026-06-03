# NPC 标签系统 - 实现总结

**实现日期**: 2026-05-25  
**版本**: 1.0

## 实现的功能

### ✅ 1. NPCAppearanceOverride (数据容器)
**文件**: `Assets/Scripts/Data/Characters/NPCAppearanceOverride.cs`

一个 ScriptableObject，用于存储 NPC 的外观与对话覆盖信息：
- **字段**:
  - `overrideSprite`: 替换的立绘/头像
  - `overrideAnimatorController`: 替换的动画控制器
  - `overrideDialogueNodes`: 覆盖的对话节点列表
  - `overrideRequiredItemGroups`: 覆盖的必需物品组列表
- **方法**:
  - `GetCopiedDialogueNodes()`: 返回对话节点的深拷贝
  - `GetCopiedRequiredItemGroups()`: 返回物品组的深拷贝

### ✅ 2. NPCLabelBase (NPC 标签基类)
**文件**: `Assets/Scripts/Data/Characters/NPCLabelBase.cs`

继承 ObjectLabel，实现 NPC 外观与对话的动态覆盖：

**生命周期**:
- `OnAttach()`: 
  - 检查并移除同类型的旧标签（单一生效规则）
  - 缓存 NPC 的原始状态（Sprite、Animator、对话数据）
  - 应用覆盖配置
  - 若对话进行中，验证节点索引并刷新 UI

- `OnDetach()`:
  - 恢复 NPC 的原始状态
  - 若对话进行中，验证节点索引并刷新 UI
  - 清空缓存

**特性**:
- 深拷贝对话数据，避免污染原始资产
- 支持 Sprite、AnimatorController、对话内容、必需物品组的覆盖
- 自动处理节点索引合法性检查

### ✅ 3. NPCObject (NPC 游戏对象)
**文件**: `Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs`

新增功能:

**字段**:
- `runtimeDialogueNodes`: 运行时对话副本
- `runtimeRequiredItemGroups`: 运行时必需物品组副本

**新增方法**:
- `InitializeRuntimeDialogueData()`: 初始化运行时副本（Awake 中调用）
- `GetCurrentDialogueNodes()`: 获取当前使用的对话列表
- `GetCurrentRequiredItemGroups()`: 获取当前使用的必需物品组
- `SetRuntimeDialogueNodes(List<DialogueNode>)`: 供标签使用，设置运行时对话
- `SetRuntimeRequiredItemGroups(List<...>)`: 供标签使用，设置运行时必需物品组
- `SetCurrentNodeIndex(int)`: 供标签使用，设置当前节点索引
- `TriggerNodeChanged()`: 供标签使用，触发节点变化事件（UI 刷新）
- `GetNPCData()`: 供标签使用，获取原始 NPCData

**修改的方法**:
- `GetCurrentContent()`: 改为读运行时副本
- `GetCurrentOptions()`: 改为读运行时副本
- `CurrentNodeHasOptions()`: 改为读运行时副本
- `ResolveStartNodeIndex()`: 改为读运行时副本的 requiredItemGroups
- `AdvanceToNextNode()`: 改为读运行时对话列表
- `TransitionToNode()`: 改为读运行时对话列表的 exitActions
- `ExecuteCurrentNodeActions()`: 改为读运行时对话列表的 enterActions
- `ApplySaveState()`: 改为验证运行时副本的节点索引合法性

**辅助方法**:
- `CopyDialogueNode()`: 深拷贝对话节点
- `CopyRequiredItemGroup()`: 深拷贝必需物品组

### ✅ 4. VillagerLabel (示例子类)
**文件**: `Assets/Scripts/Data/Characters/VillagerLabel.cs`

展示如何继承 NPCLabelBase 创建自定义标签：
```csharp
public class VillagerLabel : NPCLabelBase {
    public VillagerLabel() { }
    public VillagerLabel(NPCAppearanceOverride config) : base(config) { }
    public override string labelName => "Villager";
}
```

### ✅ 5. 存档系统扩展
**文件**: 
- `Assets/Scripts/Core/Save/GameSaveStructure.cs`
- `Assets/Scripts/Core/Save/SaveManager.cs`
- `Assets/Scripts/Core/SceneState/PlayerSceneStateCache.cs`

**GameSaveStructure.cs** - 新增字段:
- `LabelSaveData.npcOverrideConfigName`: 存储 NPCAppearanceOverride 的资产名称

**SaveManager.cs** - 修改方法:
- `SaveLabels()`: 现在会保存 NPCLabelBase 的 overrideConfig 资产名称
- `BuildLabel()`: 现在会根据资产名称加载 NPCAppearanceOverride 并恢复标签

**PlayerSceneStateCache.cs** - 修改方法:
- `CacheLabels()`: 现在会缓存 NPCLabelBase 的 overrideConfig 资产名称
- `BuildLabel()`: 现在会根据资产名称加载 NPCAppearanceOverride

## 工作流程图

```
┌─────────────────────────────────────────────────────────────┐
│                    挂载 NPCLabelBase                         │
└─────────────────────────────────────────────────────────────┘
              ↓
    ┌─────────────────────────┐
    │ 检查同类型标签          │
    │ (移除旧标签)            │
    └──────────┬──────────────┘
               ↓
    ┌─────────────────────────┐
    │ 缓存原始状态            │
    │ • Sprite                │
    │ • Animator              │
    │ • dialogueNodes (深拷贝) │
    │ • requiredItemGroups    │
    └──────────┬──────────────┘
               ↓
    ┌─────────────────────────┐
    │ 应用覆盖                │
    │ • 修改 SpriteRenderer   │
    │ • 修改 Animator         │
    │ • 设置运行时副本        │
    │ • 验证节点索引          │
    │ • 刷新 UI (如需)        │
    └──────────┬──────────────┘
               ↓
         ✓ 标签生效

        ┌──────────────────────────────────────┐
        │        标签生效期间 NPC 运作         │
        │ • 对话读取运行时副本                 │
        │ • UI 显示覆盖后的内容                │
        │ • 所有对话逻辑基于套本               │
        └──────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    卸载 NPCLabelBase                         │
└─────────────────────────────────────────────────────────────┘
              ↓
    ┌─────────────────────────┐
    │ 恢复原始状态            │
    │ • Sprite                │
    │ • Animator              │
    │ • dialogueNodes         │
    │ • requiredItemGroups    │
    └──────────┬──────────────┘
               ↓
    ┌─────────────────────────┐
    │ 验证节点索引            │
    │ 刷新 UI (如需)          │
    │ 清空缓存                │
    └──────────┬──────────────┘
               ↓
         ✓ 标签移除，NPC 恢复原状
```

## 文件清单

### 新建文件
1. `Assets/Scripts/Data/Characters/NPCAppearanceOverride.cs`
2. `Assets/Scripts/Data/Characters/NPCLabelBase.cs`
3. `Assets/Scripts/Data/Characters/VillagerLabel.cs`
4. `Docs/NPC_Label_System.md` - 使用指南
5. `Docs/NPC_Label_System_Testing.md` - 测试清单

### 修改的文件
1. `Assets/Scripts/Gameplay/Characters/NPC/NPCObject.cs` - 新增运行时副本和相关方法
2. `Assets/Scripts/Core/Save/GameSaveStructure.cs` - 扩展 LabelSaveData
3. `Assets/Scripts/Core/Save/SaveManager.cs` - 支持 NPC 标签的存档
4. `Assets/Scripts/Core/SceneState/PlayerSceneStateCache.cs` - 支持 NPC 标签的场景缓存

## 使用快速开始

### 1. 创建覆盖配置
```
右键 → Create → Game → NPCAppearanceOverride
配置 Sprite、AnimatorController、对话等
保存到 Resources/NPCAppearanceOverrides/
```

### 2. 挂载标签
```csharp
var config = Resources.Load<NPCAppearanceOverride>("NPCAppearanceOverrides/ConfigName");
var label = new VillagerLabel(config);
npc.AddLabel(label);
```

### 3. 卸载标签
```csharp
npc.RemoveLabel(label);
```

## 设计特性

| 特性 | 说明 |
|------|------|
| **单一生效** | 同类型标签只有最后一个生效，新标签自动卸载旧标签 |
| **深拷贝** | 所有对话数据都被深拷贝到运行时副本，不污染原始资产 |
| **可存档** | 标签状态和配置会被保存到存档文件 |
| **即时刷新** | 实时切换标签时 UI 自动更新 |
| **安全验证** | 节点索引始终验证合法性，避免数组越界 |
| **易扩展** | 可继承 NPCLabelBase 创建自定义标签 |

## 已知限制

1. **单一生效规则**: 同类型标签不支持堆叠，只有一个生效
2. **资产路径**: 存档读档时假设 NPCAppearanceOverride 资产在 `Resources/NPCAppearanceOverrides/` 路径
3. **引用检查**: 使用反射访问字段，需确保字段名称正确

## 测试状态

- [ ] 功能测试 (待执行)
- [ ] 边界情况测试 (待执行)
- [ ] 性能测试 (待执行)
- [ ] 集成测试 (待执行)

详见 `NPC_Label_System_Testing.md`

## 破坏性改动

⚠️ **重要**: NPCObject 的对话接口现在读取"运行时副本"而非直接访问 NPCData：
- `GetCurrentContent()` 
- `GetCurrentOptions()`
- `CurrentNodeHasOptions()`
- 等等

这改变了对话数据的数据源，但不改变 API 签名，所以外部代码不需要修改。

## 后续改进建议

1. **UI 优化**: 可在 ActionUI 中添加"标签指示器"显示当前应用的标签
2. **动画过渡**: 在 Sprite/Animator 切换时添加淡入淡出效果
3. **标签堆叠**: 如需支持多个同类型标签叠加效果，可扩展优先级系统
4. **配置验证**: 在编辑器中验证 NPCAppearanceOverride 的配置合法性
5. **动态更新**: 支持运行时修改标签的覆盖配置

---

**实现者**: GitHub Copilot  
**最后更新**: 2026-05-25
