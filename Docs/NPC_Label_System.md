# NPC 标签系统使用指南

## 概述

NPC 标签系统允许你在运行时改变 NPC 的外观和对话内容。挂载标签时 NPC 会切换到覆盖配置，卸载标签时恢复原状。

## 核心组件

### 1. NPCAppearanceOverride (ScriptableObject)
存储 NPC 的覆盖信息：
- **Sprite**: 替换的立绘/头像
- **AnimatorController**: 替换的动画控制器
- **DialogueNodes**: 覆盖的对话节点列表
- **RequiredItemGroups**: 覆盖的必需物品组列表

### 2. NPCLabelBase
继承 ObjectLabel 的基类，负责：
- 挂载时：缓存原始状态，应用覆盖
- 卸载时：恢复原始状态
- 处理"单一生效"规则（同类标签只有最后一个生效）

### 3. VillagerLabel
示例子类，展示如何使用 NPCLabelBase。

## 使用步骤

### Step 1: 创建 NPCAppearanceOverride 资产

1. 在 Project 中右键 → Create → Game → NPCAppearanceOverride
2. 命名为 `NPCAppearanceOverride_VillagerDisguise` （示例）
3. 配置内容：
   - 设置 **Override Sprite**（可选）
   - 设置 **Override Animator Controller**（可选）
   - 设置 **Override Dialogue Nodes** 列表（可选）
   - 设置 **Override Required Item Groups** 列表（可选）
4. 保存到 `Resources/NPCAppearanceOverrides/` 文件夹（便于存档时加载）

### Step 2: 在 NPC 上挂载标签

#### 编辑器配置方式（静态）
在 NPCStaticData 的 `labelBlueprints` 中添加：
```
VillagerLabel
```

#### 运行时动态挂载
```csharp
var npc = FindObjectOfType<NPCObject>();
var config = Resources.Load<NPCAppearanceOverride>("NPCAppearanceOverrides/NPCAppearanceOverride_VillagerDisguise");
var label = new VillagerLabel(config);
npc.AddLabel(label);
```

### Step 3: 卸载标签

```csharp
// 方式1：通过 RemoveLabel
npc.RemoveLabel(label);

// 方式2：通过 Detach（直接解绑）
label.Detach();
```

## 工作流程

### 挂载流程
1. 检查是否已有同类型标签，若有则先卸载
2. 缓存当前 NPC 的原始状态：
   - Sprite
   - RuntimeAnimatorController
   - dialogueNodes（深拷贝）
   - requiredItemGroups（深拷贝）
3. 应用覆盖：
   - 修改 SpriteRenderer.sprite
   - 修改 Animator.runtimeAnimatorController
   - 修改运行时对话列表
   - 如果对话正进行，验证节点索引合法性并触发 UI 刷新

### 卸载流程
1. 恢复缓存的原始状态
2. 如果对话正进行，验证节点索引合法性并触发 UI 刷新
3. 清空缓存

### 对话数据使用
NPCObject 现在使用"运行时副本"而不是直接访问 NPCData：
- `GetCurrentContent()` → 读运行时副本
- `GetCurrentOptions()` → 读运行时副本
- `ResolveStartNodeIndex()` → 读运行时副本的 requiredItemGroups
- 其他对话相关方法也都更新为读运行时副本

## 存档/读档

标签状态会被保存到存档文件：
- SaveManager 会保存所有标签及其配置信息
- 读档时根据配置名称重新加载 NPCAppearanceOverride 资产
- NPC 的原始对话不会被污染

## 创建自定义 NPC 标签子类

```csharp
[Serializable]
public class MyCustomNPCLabel : NPCLabelBase {
    
    public MyCustomNPCLabel() { }

    public MyCustomNPCLabel(NPCAppearanceOverride config) : base(config) { }

    public override string labelName => "MyCustomLabel";
    
    // 可以重写 OnAttach/OnDetach 添加额外逻辑
    public override void OnAttach(ILabelOwner owner) {
        base.OnAttach(owner);
        // 自定义初始化逻辑
    }

    public override void OnDetach(ILabelOwner owner) {
        base.OnDetach(owner);
        // 自定义清理逻辑
    }
}
```

## 注意事项

1. **深拷贝**: NPCAppearanceOverride 中的对话列表会被深拷贝到运行时副本，不会污染原始资产。

2. **单一生效规则**: 同类型的标签只有最后挂载的会生效。新标签挂载时会自动卸载旧标签。

3. **对话进行中的切换**: 如果对话正进行中挂载/卸载标签，系统会自动验证当前节点索引是否仍然合法，并刷新 UI。

4. **资产路径**: 存档时会保存覆盖配置的资产名称。读档时需要从 `Resources/NPCAppearanceOverrides/` 路径加载，确保资产放在正确位置。

5. **Sprite 替换**: 如果 NPCAppearanceOverride 没有配置 Override Sprite，则不会改变原来的 Sprite。其他字段同理。

## 示例：村民变装

创建流程：
1. 创建 NPCAppearanceOverride 资产 → NPCAppearanceOverride_Villager
2. 配置新的 Sprite（村民服装）
3. 配置新的 RuntimeAnimatorController（村民动画）
4. 添加新的对话节点（村民说的话）
5. 在场景中给 NPC 挂载 VillagerLabel

结果：
- NPC 立绘变成村民样子
- NPC 动画变成村民动作
- NPC 对话变成村民对话
- 卸载标签后完全恢复
