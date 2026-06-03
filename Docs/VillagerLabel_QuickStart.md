# VillagerLabel - 使用指南

## 概述

VillagerLabel 是改进后的 NPC 标签示例，所有 VillagerLabel 实例均指向同一个 NPCAppearanceOverride 对象（VillagerConfig）。

## 快速开始

### 步骤 1: 生成测试数据

在 Unity 编辑器中：
1. 菜单栏 → **Tools** → **Generate NPC Test Data** → **Create Villager Config**
2. 这会自动在 `Assets/Resources/NPCAppearanceOverrides/` 路径下创建 `VillagerConfig.asset` 文件

### 步骤 2: 配置覆盖内容（可选）

打开生成的 `VillagerConfig.asset`，你可以配置：
- **Override Sprite**: 替换的村民立绘
- **Override Animator Controller**: 替换的动画控制器
- **Override Dialogue Nodes**: 村民的对话内容（已预填充示例）
- **Override Required Item Groups**: 对话触发条件（已预填充示例）

### 步骤 3: 在代码中使用

```csharp
// 获取 NPC
var npc = FindObjectOfType<NPCObject>();

// 创建标签（会自动加载 VillagerConfig）
var label = new VillagerLabel();

// 挂载标签
npc.AddLabel(label);

// ... NPC 现在显示村民外观和对话

// 卸载标签
npc.RemoveLabel(label);

// ... NPC 恢复原来的外观和对话
```

## 关键特性

### ✅ 单一配置引用

所有 VillagerLabel 实例都指向同一个 NPCAppearanceOverride 对象：

```csharp
var label1 = new VillagerLabel();
var label2 = new VillagerLabel();

// label1 和 label2 内部的 overrideConfig 指向同一个对象
// 这样可以节省内存，避免重复加载
```

### ✅ 自动加载

VillagerLabel 的构造函数会自动从 `Resources/NPCAppearanceOverrides/VillagerConfig` 加载配置：

```csharp
public VillagerLabel() {
    // 自动加载 VillagerConfig
    // 如果加载失败会在 Console 中显示错误日志
}
```

### ✅ 错误处理

如果配置文件未找到，VillagerLabel 会输出错误日志：

```
VillagerLabel: Failed to load NPCAppearanceOverride from path: NPCAppearanceOverrides/VillagerConfig
```

## 文件结构

```
Assets/
├── Resources/
│   └── NPCAppearanceOverrides/
│       └── VillagerConfig.asset  ← 由工具生成
├── Scripts/
│   ├── Data/Characters/
│   │   ├── VillagerLabel.cs  ← 改进后的标签类
│   │   ├── NPCLabelBase.cs
│   │   └── NPCAppearanceOverride.cs
│   └── Editor/
│       └── NPCTestDataGenerator.cs  ← 编辑器工具
```

## 编辑器菜单命令

### Create Villager Config
```
Tools → Generate NPC Test Data → Create Villager Config
```
生成 VillagerConfig.asset 文件

### Delete Villager Config
```
Tools → Generate NPC Test Data → Delete Villager Config
```
删除 VillagerConfig.asset 文件

### Open Villager Config
```
Tools → Generate NPC Test Data → Open Villager Config
```
在 Inspector 中打开 VillagerConfig.asset

## 预填充的测试对话

VillagerConfig 包含 4 个对话节点：

```
节点 0: "欢迎来到村庄！你是新来的吗？"
  ├─ 选项 1: "我是村民。" → 节点 1
  └─ 选项 2: "告诉我关于这个地方的事情。" → 节点 2

节点 1: "是的，我是这个村子里的一个普通村民。" → 节点 3

节点 2: "这是一个美好的地方，有许多有趣的人和事。" → 节点 3

节点 3: "有什么我可以帮你的吗？" → 结束
```

## 常见问题

### Q: 为什么生成的配置中 Sprite 和 Animator Controller 是空的？

A: 这些字段需要你在编辑器中手动配置。生成工具只创建对话框架。

### Q: 我可以为不同的 NPC 创建不同的标签子类吗？

A: 可以。按照 VillagerLabel 的模式创建新的子类，指向不同的配置资产：

```csharp
[Serializable]
public class GuardLabel : NPCLabelBase {
    private static NPCAppearanceOverride cachedConfig;
    private const string CONFIG_RESOURCE_PATH = "NPCAppearanceOverrides/GuardConfig";
    
    public GuardLabel() {
        if (cachedConfig == null) {
            cachedConfig = Resources.Load<NPCAppearanceOverride>(CONFIG_RESOURCE_PATH);
        }
        var field = typeof(NPCLabelBase).GetField("overrideConfig", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) {
            field.SetValue(this, cachedConfig);
        }
    }
    
    public override string labelName => "Guard";
}
```

### Q: 将配置资产放在 Resources 路径下有什么好处？

A: Resources 路径下的资产会被包含在打包中，运行时可以通过 `Resources.Load` 快速加载，不需要额外的资产管理。

### Q: 能否在不创建编辑器工具的情况下手动创建 VillagerConfig？

A: 可以。在编辑器中：
1. 右键 → Create → Game → NPCAppearanceOverride
2. 命名为 VillagerConfig
3. 放入 `Assets/Resources/NPCAppearanceOverrides/` 文件夹
4. 配置想要的内容

## 下一步

- 试试创建其他标签子类（GuardLabel、MerchantLabel 等）
- 为每个标签子类创建相应的配置资产
- 将标签添加到 NPCStaticData 的 labelBlueprints 中进行静态配置

## 参考

- [NPCLabelBase 文档](NPC_Label_System.md)
- [API 快速参考](NPC_Label_System_API_Reference.md)
- [实现细节](NPC_Label_System_Implementation.md)
