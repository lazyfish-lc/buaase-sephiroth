
# 接口文档

## 物品使用

- 物品创建（运行时）：
	- `Item.Create(string itemName, ItemType itemType = ItemType.Normal, float recoverAmount = 0f)`
		- `itemType == ItemType.Recover` 时返回 `RecoverItem`，并写入 `recoverAmount`。
	- `Item.CreateFrom(Item blueprint)`
		- 依据蓝图的 `itemType` 与子类字段创建实例；若为 `RecoverItem` 蓝图，会复制 `recoverAmount`。

- 使用恢复类物品：
	- `PlayerSmallObject.UseRecoverItem(RecoverItem item)`
		- 效果：增加 `Health`，并从背包移除该物品。

- 物品存档结构：
	- `ItemSaveData`
		- `itemName`：物品名称。
		- `itemType`：物品类型（如 `Normal` / `Recover`）。
		- `recoverAmount`：恢复类物品的恢复量。
		- `extraJson`：预留扩展字段（子类自定义数据）。

## 教程触发（后端 -> UI）

- 事件：
	- `DialogueNodeActionSOActions.TutorialPopupRequested`
		- 签名：`Action<NPCObject, string, int>`
		- 参数含义：
			- `NPCObject`：当前交互 NPC。
			- `string actionId`：教程动作 ID（用于去重）。
			- `int tutorialIndex`：全局教程表中的下标。
		- 触发位置：`DialogueNodeActionSOActions.ExecuteUIActions(...)`。

- 触发条件：
	- `actionId` 未出现过（防重复）。
	- `tutorialIndex >= 0` 时，该教程会被标记为“历史教程可见”。

## 历史教程（UI 读取接口）

- 可见教程索引：
	- `DialogueNodeActionSOActions.GetVisibleTutorialIndices()`
		- 返回当前“历史教程可见”的索引集合。
	- 可见性维护：
		- `DialogueNodeActionSOActions.ApplyVisibleTutorialIndices(IEnumerable<int> indices)`
		- 由存档读取时恢复。

- UI 查询历史教程示例：
	- 获取可见索引后，过滤 `TutorialContentTable` 中的条目。

```csharp
public class TutorialHistoryUI : MonoBehaviour {
	[SerializeField] private TutorialContentTable tutorialTable;

	public List<TutorialContent> GetVisibleTutorials() {
		var result = new List<TutorialContent>();
		if (tutorialTable == null || tutorialTable.tutorials == null) return result;

		foreach (var index in DialogueNodeActionSOActions.GetVisibleTutorialIndices()) {
			if (index < 0 || index >= tutorialTable.tutorials.Count) continue;
			result.Add(tutorialTable.tutorials[index]);
		}

		return result;
	}
}
```

## 教程静态数据

- 全局教程表（静态数据）：
	- `TutorialContentTable`
		- `List<TutorialContent> tutorials`
	- `TutorialContent`
		- `title`：标题
		- `body`：正文

- UI 通过索引查询示例：
	- 将 `TutorialContentTable` 以序列化字段注入 UI 逻辑，再用 `tutorialIndex` 取出内容。

```csharp
public class TutorialPopupUI : MonoBehaviour {
	[SerializeField] private TutorialContentTable tutorialTable;

	public void ShowTutorial(int tutorialIndex) {
		if (tutorialTable == null || tutorialTable.tutorials == null) return;
		if (tutorialIndex < 0 || tutorialIndex >= tutorialTable.tutorials.Count) return;

		TutorialContent data = tutorialTable.tutorials[tutorialIndex];
		// 这里用 data.title / data.body 更新 UI
	}
}
```
