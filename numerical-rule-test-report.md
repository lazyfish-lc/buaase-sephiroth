# 数值规则测试报告

日期：2026-04-24

## 一、测试范围

本轮主要针对以下模块补充并验证 EditMode 测试：

- `SmallObjectProperty.SetValue`
- `NumericalMaintainConstantRule.Notify`
- `NumericalRuleFactory`
- `NumericalRuleManager`

## 二、分支与版本说明

- 工作分支：`numerical-rule-tests`
- 当前本地已知基线提交：`e8ec9ab`
- 本地 `develop` 与本地记录的 `origin/develop` 一致

说明：

我尝试执行了两次 `git fetch origin` 以获取远端最新 `develop`，但两次都因网络问题失败，错误信息如下：

`Recv failure: Connection was reset`

因此，本次复测基于当前本地已知的最新 `develop` 代码，而不是一次成功 fetch 之后的远端最新状态。

## 三、测试准备

为支持 Unity EditMode 测试，本次新增了以下内容：

- `Assets/Scripts/Sephiroth.Runtime.asmdef`
- `Assets/Tests/EditMode/EditModeTests.asmdef`
- `Assets/Tests/EditMode/NumericalRuleTests.cs`

本地测试环境：

- Unity Editor：`6000.3.11f1`
- Unity License：`Unity Personal`，已通过 Unity Hub 激活

## 四、本地执行方式

实际执行命令如下：

```powershell
Unity.exe `
  -batchmode `
  -nographics `
  -projectPath . `
  -runTests `
  -testPlatform EditMode `
  -testResults Temp\EditModeResults.xml `
  -logFile Temp\UnityEditMode.log
```

仓库内可直接查看的相关路径为：

- `Assets/Scripts/Sephiroth.Runtime.asmdef`
- `Assets/Tests/EditMode/EditModeTests.asmdef`
- `Assets/Tests/EditMode/NumericalRuleTests.cs`
- `Assets/Scripts/Controller/NumericalRuleFactory.cs`
- `Temp/UnityEditMode.log`

说明：

Unity 实际测试结果 XML 最终写入了 Unity 默认用户结果目录，而不是稳定落在仓库 `Temp/` 下。

## 五、测试结果汇总

- 测试总数：`15`
- 通过：`13`
- 失败：`2`
- 跳过：`0`

结论：

当前新增测试已经能够在本地 Unity 环境中正常执行，并成功覆盖了本次 leader 提到的核心目标方法；其中大部分测试已通过，剩余 2 个失败点定位到了 `NumericalRuleFactory` 的真实实现问题。

## 六、失败用例

本轮失败的测试有 2 个：

1. `NumericalRuleFactoryBuildMaintainConstantRule_CreatesRuleForEachTermAndAttachesInputs`
2. `NumericalRuleFactoryBuildRule_WhenMaintainRuleIsValid_ReturnsBaseRuleList`

## 七、失败原因分析

这两个失败点不是测试环境问题，也不是测试夹具问题，而是 `NumericalRuleFactory` 当前实现中确实存在返回值设计缺陷。

问题位置：

- `Assets/Scripts/Controller/NumericalRuleFactory.cs`

在 `BuildMaintainConstantRule` 的循环中，代码已经完成了以下动作：

- 创建 `NumericalMaintainConstantRule rule`
- 填充 `inputProperty`
- 填充 `outputProperties`
- 填充 `outputWeights`
- 通过 `rule.inputProperty.AddNumericalRule(rule)` 将规则挂载到输入属性

但是没有把新构造出来的 `rule` 加入返回列表 `rules`，也就是缺少类似下面这一句：

```csharp
rules.Add(rule);
```

因此会导致如下现象：

1. `BuildMaintainConstantRule(..., out rules)` 返回 `true`，但 `rules.Count == 0`
2. `BuildRule(..., out rules)` 也会得到空列表
3. `NumericalRuleManager.BuildAndRegisterRules(...)` 看起来仍然可能生效，因为 Factory 在构建时已经通过副作用把 rule 挂到了 property 上

也就是说，目前 `NumericalRuleFactory` 的“构建并返回规则列表”语义与“构建过程中直接注册规则”的语义混在了一起，返回结果与实际副作用不一致，这正是当前两条失败测试暴露出来的问题。

## 八、已验证通过的内容

以下内容在本地复测中已经通过：

### 1. `SmallObjectProperty.SetValue`

- 合法值修改成功
- 越界值被拒绝
- 被规则拦截时不更新值，也不会继续通知

### 2. `NumericalMaintainConstantRule.Notify`

- 能按权重正确分配变化量
- 当输出属性将越界时，不执行联动修改
- 当输出权重和为 0 时，不执行联动修改

### 3. `NumericalRuleManager`

- `RegisterRule`
- `RegisterRules`
- `BuildAndRegisterRules`
- 非法输入 guard 分支

以上相关测试均已通过。

## 九、当前结论

本轮工作已经完成了以下目标：

1. 补齐了数值守恒规则相关的 EditMode 测试框架
2. 在本地 Unity 环境中实际跑通了测试
3. 成功定位出 `NumericalRuleFactory` 的一个真实缺陷

当前整体状态可以概括为：

- 测试已经“能跑”
- 大部分测试已经“跑通”
- 剩余失败点已经明确定位到业务实现，不再是测试环境问题

## 十、建议的下一步

建议后续直接修复 `NumericalRuleFactory` 中未将构造出的 `rule` 加入返回列表的问题，即补上：

```csharp
rules.Add(rule);
```

修复后重新执行同一组 EditMode 测试，预期即可进一步验证是否能够全部通过。
