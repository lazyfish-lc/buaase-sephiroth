# Label Rule Test Report

## 基本信息

- 测试对象：标签规则相关逻辑
- 测试文件：`Assets/Tests/EditMode/LabelRuleTests.cs`
- 测试类型：Unity EditMode 单元测试
- 测试时间：2026-04-26 23:41:13 CST
- 测试分支：`develop`
- 最新提交：`d2d1fed Merge pull request #62 from lazyfish-lc/feature/ui`

## 更新检查

测试前已检查远端更新，并将本地 `develop` 快进到最新的 `origin/develop`。

远端更新前：

```text
93ef84f Merge pull request #61 from lazyfish-lc/feature/battle-sys
```

远端更新后：

```text
d2d1fed Merge pull request #62 from lazyfish-lc/feature/ui
```

本次新增的 `LabelRuleTests.cs` 未与远端更新发生冲突。

## 测试覆盖范围

本次针对标签规则新增 9 个测试用例，主要覆盖以下行为：

- `ObjectLabel.AttachToOwner` 能正确绑定实现了标签接口的回调。
- `ObjectLabel.Detach` 能正确解绑事件、清空 owner，并调用 `OnDetach`。
- 标签从旧 owner 转移到新 owner 时，会先从旧 owner 解绑。
- `AttachToOwner(null)` 不会产生错误绑定。
- `SmallObject.AddLabel` 能保存标签、绑定事件，并防止重复添加。
- `SmallObject.RemoveLabel` 能移除标签并解绑事件。
- `BigObject.AddLabel` 能保存标签、绑定事件，并防止重复添加。
- `BigObject.RemoveLabel` 能移除标签并解绑事件。
- `AttachToSmallObject` 兼容入口能复用 `AttachToOwner` 的绑定逻辑。

## 执行命令

```powershell
& "$env:USERPROFILE\UnityEditors\6000.3.11f1\Editor\Unity.exe" `
  -batchmode `
  -nographics `
  -projectPath . `
  -runTests `
  -testPlatform editmode `
  -testResults Temp\EditModeResults.xml `
  -logFile Temp\UnityEditMode.log
```

说明：本项目中 Unity Test Runner 需要去掉 `-quit`，否则 Unity 会在完成项目导入后直接退出，无法完整执行测试。

## 测试结果

总体结果：

```text
Total: 24
Passed: 24
Failed: 0
Skipped: 0
Result: Passed
Duration: 0.0878617s
```

分类结果：

```text
LabelRuleTests: 9 passed, 0 failed
NumericalRuleTests: 15 passed, 0 failed
```

Unity 日志确认：

```text
Test run completed. Exiting with code 0 (Ok). Run completed.
```

## 结论

标签规则测试已在最新 `develop` 上通过。新增测试覆盖了标签生命周期、事件订阅和解绑、宿主对象集成以及重复添加防护等核心行为，可以作为后续修改标签系统时的回归测试基础。

## 备注

运行 Unity EditMode 测试时，TextMesh Pro 字体资产会被 Unity 自动写入临时变化。本次验证后已恢复这些无关资产，当前工作区只保留新增的标签规则测试及本报告文件。
