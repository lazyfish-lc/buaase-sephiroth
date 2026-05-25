# NPC 标签系统 - 测试验证清单

## 环境准备

- [ ] 代码编译成功（无错误）
- [ ] Unity 编辑器已刷新（如需要）
- [ ] 找到一个测试用的 NPC 场景

## 基础功能测试

### 1. 创建 NPCAppearanceOverride 资产文件
- [ ] 在 Assets/Resources 中创建文件夹 `NPCAppearanceOverrides`
- [ ] 右键创建 → Game → NPCAppearanceOverride
- [ ] 命名为 `TestVillagerOverride`
- [ ] 在该资产中配置：
  - [ ] Override Sprite（选择一个不同的精灵）
  - [ ] Override Animator Controller（选择一个不同的动画控制器）
  - [ ] Override Dialogue Nodes（添加至少一个对话节点，内容改为"我是伪装的村民"）

### 2. Sprite 替换测试
- [ ] 在场景中找到一个 NPC
- [ ] 记录其初始 Sprite（应该是原始的）
- [ ] 运行时动态挂载标签：
  ```csharp
  var config = Resources.Load<NPCAppearanceOverride>("NPCAppearanceOverrides/TestVillagerOverride");
  var label = new VillagerLabel(config);
  npc.AddLabel(label);
  ```
- [ ] 验证 NPC 的 Sprite 已改变为配置中的 Sprite
- [ ] 卸载标签
- [ ] 验证 NPC 的 Sprite 已恢复为原始 Sprite

### 3. Animator 替换测试
- [ ] 记录初始 Animator Controller
- [ ] 挂载标签（同上）
- [ ] 验证 NPC 的 Animator Controller 已改变
- [ ] 卸载标签
- [ ] 验证 Animator Controller 已恢复

### 4. 对话替换测试
- [ ] 与 NPC 开始对话
- [ ] 验证当前对话内容仍为原始内容
- [ ] 卸载对话中的标签或在对话中途挂载标签
- [ ] 挂载标签（在对话中）
- [ ] 验证对话内容立即更新为覆盖内容
- [ ] 验证 UI 已刷新显示新对话
- [ ] 继续对话流程，验证新的对话节点能正常跳转
- [ ] 卸载标签
- [ ] 验证对话内容恢复为原始内容

### 5. 单一生效规则测试
- [ ] 挂载第一个 VillagerLabel
- [ ] 验证效果已应用
- [ ] 再挂载一个不同配置的 VillagerLabel（新建另一个 Override 资产）
- [ ] 验证第一个标签已自动卸载
- [ ] 验证只有第二个标签的效果生效
- [ ] 卸载第二个标签
- [ ] 验证所有覆盖都已恢复

### 6. 存档/读档测试
- [ ] 挂载一个标签到 NPC
- [ ] 验证效果已应用
- [ ] 保存游戏（SaveManager.Instance.SaveGame()）
- [ ] 卸载标签（或重启场景）
- [ ] 验证效果已消失
- [ ] 读档（SaveManager.Instance.LoadGame()）
- [ ] 验证标签已自动重新挂载（或从存档数据恢复）
- [ ] 验证效果已重新应用
- [ ] 验证对话状态与保存时一致

## 高级功能测试

### 7. 编辑器配置使用（LabelFactory）
- [ ] 在某个 SmallObject 的 staticData 中的 labelBlueprints 中添加 `VillagerLabel`
- [ ] 运行场景，验证标签自动挂载
- [ ] 验证效果已应用

### 8. 自定义子类测试
- [ ] 创建一个新的子类继承 NPCLabelBase
- [ ] 配置一个新的 NPCAppearanceOverride 资产
- [ ] 测试挂载新子类
- [ ] 验证效果与 VillagerLabel 表现一致

### 9. 边界情况测试
- [ ] **对话中途标签切换**：在对话中挂载/卸载标签，验证节点索引不越界
- [ ] **多个不同类型标签共存**：同时挂载 VillagerLabel 和其他类型的标签（如 GlowLabel），验证不互相干扰
- [ ] **对话节点数差异**：覆盖配置的对话节点数少于原始数据，验证节点索引合法性检查
- [ ] **空覆盖配置**：NPCAppearanceOverride 中不配置任何字段，验证不会崩溃

### 10. UI 集成测试
- [ ] 在 ActionUI 显示的状态下挂载标签
- [ ] 验证 UI 中显示的对话文本立即更新
- [ ] 验证 UI 中显示的 Sprite 立即更新
- [ ] 卸载标签
- [ ] 验证 UI 中的内容立即恢复

## 性能测试

- [ ] [ ] 在场景中有多个 NPC 的情况下，频繁挂载/卸载标签，验证帧率不显著下降
- [ ] [ ] 验证内存使用不会持续增长

## 文档验证

- [ ] [ ] NPC_Label_System.md 文档清晰准确
- [ ] [ ] 代码注释充分
- [ ] [ ] 示例代码能正常运行

## 已知限制/注意事项

- [ ] 确认同类型标签只有最后一个生效（不支持堆叠）
- [ ] 确认存档系统需要正确放置 NPCAppearanceOverride 资产到 `Resources/NPCAppearanceOverrides/` 文件夹
- [ ] 确认深拷贝的对话数据不会影响原始资产

---

## 测试报告模板

**测试日期**: ___________  
**测试者**: ___________  
**环境**: Unity ________, C# ________  

**测试结果**:
- [ ] 全部通过
- [ ] 部分通过（请注明失败项）
- [ ] 失败（请附上错误日志）

**失败项说明**:
```
[失败项编号]: [失败描述]
[复现步骤]
[预期结果 vs 实际结果]
[错误日志]
```

**备注**:
___________________________________________________________________________
