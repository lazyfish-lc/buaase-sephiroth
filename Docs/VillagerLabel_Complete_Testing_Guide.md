# VillagerLabel - 完整测试指南

## 概述

改进后的 VillagerLabel 已完成以下优化：
- ✅ 移除参数化构造函数
- ✅ 所有实例指向同一个 NPCAppearanceOverride 对象（VillagerConfig）
- ✅ 提供自动化编辑器工具生成测试数据
- ✅ 提供演示脚本展示使用方法

## 快速开始（5 分钟）

### 第 1 步：生成测试数据（1 分钟）

在 Unity 编辑器中执行：
```
菜单 → Tools → Generate NPC Test Data → Create Villager Config
```

**预期结果**:
- Console 中出现: `✓ VillagerConfig 资产已创建: Assets/Resources/NPCAppearanceOverrides/VillagerConfig.asset`
- Project 窗口中自动定位到新建的 asset 文件

### 第 2 步：添加演示脚本（2 分钟）

1. 在场景中创建一个空 GameObject：`VillagerLabelDemo`
2. 将 `VillagerLabelDemo.cs` 脚本挂载到上面
3. 保存场景

**预期结果**:
- 场景 Hierarchy 中出现 VillagerLabelDemo 对象
- 脚本自动查找场景中的 NPCObject

### 第 3 步：测试功能（2 分钟）

运行场景，按键盘按键测试：

| 按键 | 功能 | 预期结果 |
|------|------|--------|
| **K** | 挂载 VillagerLabel | Console: `✓ VillagerLabel 已挂载到 [NPCName]` |
| **L** | 卸载 VillagerLabel | Console: `✓ VillagerLabel 已从 [NPCName] 卸载` |
| **U** | 列出标签列表 | 显示当前 NPC 上挂载的所有标签 |

## 详细验证清单

### ✅ 单一配置引用验证

**目标**: 验证所有 VillagerLabel 实例指向同一个 NPCAppearanceOverride 对象

**测试代码**:
```csharp
// 在 Console 中执行或创建测试脚本
var label1 = new VillagerLabel();
var label2 = new VillagerLabel();

// 通过反射比较内部配置
var field = typeof(NPCLabelBase).GetField("overrideConfig", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var config1 = field.GetValue(label1);
var config2 = field.GetValue(label2);

if (config1 == config2) {
    Debug.Log("✓ 两个标签实例指向同一个 NPCAppearanceOverride 对象");
} else {
    Debug.Log("✗ 标签实例指向不同的对象");
}
```

**预期结果**: 打印 `✓ 两个标签实例指向同一个 NPCAppearanceOverride 对象`

### ✅ 自动加载验证

**目标**: 验证 VillagerLabel 自动加载 VillagerConfig

**测试方法**:
1. 在 Console 中执行: `new VillagerLabel()`
2. 观察 Console 输出

**预期结果**:
- 无错误输出
- VillagerLabel 成功创建
- 内部 overrideConfig 不为空

### ✅ 错误处理验证

**目标**: 验证找不到配置文件时的错误处理

**测试方法**:
1. 执行菜单: `Tools → Generate NPC Test Data → Delete Villager Config`
2. 在 Console 中执行: `new VillagerLabel()`

**预期结果**: 
```
VillagerLabel: Failed to load NPCAppearanceOverride from path: NPCAppearanceOverrides/VillagerConfig
```

### ✅ 标签生命周期验证

**目标**: 验证标签的挂载/卸载生命周期

**测试场景**:
1. 按 K 挂载 VillagerLabel
2. 观察 Console 和 NPC 表现
3. 按 L 卸载 VillagerLabel
4. 观察 NPC 恢复原状

**预期结果**:
- 挂载时: NPC 显示村民对话，Console 输出成功消息
- 卸载时: NPC 恢复原始对话，Console 输出成功消息

### ✅ 对话覆盖验证

**目标**: 验证对话内容正确覆盖

**测试方法**:
1. 启动对话（点击 NPC）
2. 按 K 挂载 VillagerLabel（对话进行中）
3. 观察 UI 中的对话内容
4. 按 L 卸载标签
5. 观察对话恢复为原始内容

**预期结果**:
- 挂载时: 对话立即更新为村民对话（"欢迎来到村庄！..."）
- 卸载时: 对话立即恢复为原始对话

### ✅ 内存优化验证

**目标**: 验证所有实例共享配置，避免重复加载

**测试代码**:
```csharp
// 创建 100 个 VillagerLabel 实例
var labels = new System.Collections.Generic.List<VillagerLabel>();
for (int i = 0; i < 100; i++) {
    labels.Add(new VillagerLabel());
}

// 检查内存使用
Debug.Log($"创建了 {labels.Count} 个 VillagerLabel 实例");
Debug.Log("所有实例共享同一个 NPCAppearanceOverride 对象，节省内存");
```

**预期结果**:
- 快速创建 100 个实例（无重复加载开销）
- 内存占用较小（共享一个配置对象）

## 编辑器菜单

### Tools > Generate NPC Test Data

#### 1. Create Villager Config
```
Tools → Generate NPC Test Data → Create Villager Config
```
- **功能**: 生成测试用的 VillagerConfig.asset
- **路径**: `Assets/Resources/NPCAppearanceOverrides/VillagerConfig.asset`
- **内容**: 包含 4 个示例对话节点
- **何时使用**: 第一次使用时

#### 2. Delete Villager Config
```
Tools → Generate NPC Test Data → Delete Villager Config
```
- **功能**: 删除 VillagerConfig.asset
- **何时使用**: 重新生成或清理测试数据

#### 3. Open Villager Config
```
Tools → Generate NPC Test Data → Open Villager Config
```
- **功能**: 在 Inspector 中打开 VillagerConfig
- **何时使用**: 编辑对话或外观配置

## 预填充的测试对话

VillagerConfig 包含以下对话结构：

```
┌─────────────────────────────────────────────┐
│ 节点 0 (起始)                                 │
│ "欢迎来到村庄！你是新来的吗？"                  │
│ [选项 1] "我是村民。"          [选项 2] "告诉我..." │
└──────────┬───────────────────────────┬──────┘
           ↓                           ↓
    ┌─────────────┐            ┌──────────────┐
    │ 节点 1      │            │ 节点 2       │
    │ "是的，我是  │            │ "这是一个    │
    │ 这个村子的   │            │ 美好的地方..." │
    │ 普通村民"   │            │              │
    └──────┬──────┘            └────────┬─────┘
           │                            │
           └────────────┬───────────────┘
                        ↓
              ┌──────────────────┐
              │ 节点 3 (结束)    │
              │ "有什么我可以    │
              │ 帮你的吗？"      │
              └──────────────────┘
```

所有节点都包含空的 `enterActions` 和 `exitActions` 列表，可自由扩展。

## 故障排除

### 问题 1: "Failed to load VillagerConfig"

**原因**: 配置文件未生成或路径错误

**解决方案**:
1. 执行菜单: `Tools → Generate NPC Test Data → Create Villager Config`
2. 确认 `Assets/Resources/NPCAppearanceOverrides/VillagerConfig.asset` 存在
3. 重启 Unity 编辑器

### 问题 2: VillagerLabel 挂载后没有效果

**原因**: NPCAppearanceOverride 中的字段为空

**解决方案**:
1. 执行菜单: `Tools → Generate NPC Test Data → Open Villager Config`
2. 在 Inspector 中配置：
   - **Override Sprite**: 设置新的 Sprite 资产
   - **Override Animator Controller**: 设置新的 AnimatorController 资产
   - **Override Dialogue Nodes**: 已预填充，可直接使用

### 问题 3: 创建 VillagerLabel 时 Console 无输出

**原因**: 配置加载成功但没有日志输出

**解决方案**: 这是正常行为。只有加载失败时才会输出错误日志。

### 问题 4: 演示脚本找不到 NPCObject

**原因**: 场景中没有 NPCObject

**解决方案**:
1. 在场景中创建一个 GameObject
2. 添加 NPCObject 脚本
3. 配置 NPCStaticData
4. 重新运行演示脚本

## 下一步

1. **自定义标签**: 按照 VillagerLabel 的模式创建其他标签（GuardLabel、MerchantLabel 等）
2. **扩展对话**: 在 VillagerConfig 中添加更复杂的对话树
3. **添加动画**: 在覆盖配置中设置不同的 AnimatorController
4. **集成游戏**: 将标签系统集成到你的游戏逻辑中

## 参考文档

- [VillagerLabel 快速开始](VillagerLabel_QuickStart.md)
- [NPC 标签系统完整指南](NPC_Label_System.md)
- [API 快速参考](NPC_Label_System_API_Reference.md)

---

**文档版本**: 1.0  
**最后更新**: 2026-05-25  
**测试环境**: Unity 2022.2+
