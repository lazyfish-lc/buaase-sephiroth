using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;

    [SerializeField] private string saveFileName = "save.json";

    private GameSavePackage pendingLoad;                     // 用于 LoadGame 的跨场景待恢复包
    private Dictionary<string, GameSavePackage> sceneStates  // 自动保存：sceneName → 最近一次离开时的状态
        = new Dictionary<string, GameSavePackage>(StringComparer.Ordinal);
    private bool isLoadingSave;
    private Coroutine loadingFlagRoutine;

    public bool IsLoadingSave => isLoadingSave;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 构建当前场景的存档包并存入内存（按场景名索引）。
    /// 切换场景时会自动恢复离开时的状态，标签/物品等不会丢失。
    /// 应在切换场景前调用。
    /// </summary>
    public void SaveCurrentToPending() {
        var package = BuildSavePackage();
        if (package == null) return;
        sceneStates[package.sceneName] = package;
        Debug.Log($"[SaveManager] 已保存场景状态: {package.sceneName}");
    }

    public void SaveGame() {
        SaveGame(saveFileName);
    }

    public void SaveGame(string fileName) {
        var package = BuildSavePackage();
        if (package == null) {
            Debug.LogWarning("SaveManager: BuildSavePackage failed.");
            return;
        }

        string json = JsonUtility.ToJson(package, true);
        File.WriteAllText(GetSavePath(fileName), json);
        Debug.Log($"SaveManager: saved to {GetSavePath(fileName)}");
    }

    public void LoadGame() {
        LoadGame(saveFileName);
    }

    public bool HasSaveFile() {
        return File.Exists(GetSavePath(saveFileName));
    }

    public void LoadGame(string fileName) {
        string path = GetSavePath(fileName);
        if (!File.Exists(path)) {
            Debug.LogWarning($"SaveManager: save file not found at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        var package = JsonUtility.FromJson<GameSavePackage>(json);
        if (package == null) {
            Debug.LogWarning("SaveManager: failed to parse save file.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (!string.Equals(currentScene, package.sceneName, StringComparison.Ordinal)) {
            pendingLoad = package;
            isLoadingSave = true;
            // OnSceneLoaded 已在 Awake 中全局订阅，无需重复
            FindFirstObjectByType<LoadingUI>().LoadScene(package.sceneName);
            return;
        }

        isLoadingSave = true;
        ApplySavePackage(package);
        ScheduleClearLoadingFlag();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        string sceneName = scene.name;

        // LoadGame 跨场景恢复优先（手动读档优先级高于自动保存）
        if (pendingLoad != null && string.Equals(sceneName, pendingLoad.sceneName, StringComparison.Ordinal))
        {
            Debug.Log($"[SaveManager] 恢复 LoadGame 存档: {sceneName}");
            ApplySavePackage(pendingLoad);
            pendingLoad = null;
            // 同时清除对应场景的自动保存，避免覆盖读档结果
            sceneStates.Remove(sceneName);
            ScheduleClearLoadingFlag();
            return;
        }

        // 自动恢复：离开场景时保存的状态
        if (sceneStates.TryGetValue(sceneName, out var saved))
        {
            Debug.Log($"[SaveManager] 自动恢复场景状态: {sceneName}");
            ApplySavePackage(saved);
            sceneStates.Remove(sceneName);
            ScheduleClearLoadingFlag();
        }
    }

    private void ScheduleClearLoadingFlag() {
        if (loadingFlagRoutine != null) {
            StopCoroutine(loadingFlagRoutine);
        }
        loadingFlagRoutine = StartCoroutine(ClearLoadingFlagAfterFrame());
    }

    private IEnumerator ClearLoadingFlagAfterFrame() {
        yield return null;
        isLoadingSave = false;
        loadingFlagRoutine = null;
    }

    private string GetSavePath(string fileName) {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private GameSavePackage BuildSavePackage() {
        var package = new GameSavePackage {
            sceneName = SceneManager.GetActiveScene().name,
            isPresentTime = GameSceneManager.Instance != null ? GameSceneManager.Instance.isPresent : true,
            objectStates = new List<SmallObjectSaveData>(),
            triggeredTutorialIds = new List<string>(DialogueNodeActionSOActions.GetTriggeredTutorialIds()),
            visibleTutorialIndices = new List<int>(DialogueNodeActionSOActions.GetVisibleTutorialIndices())
        };

        var smallObjects = FindAllSmallObjects();
        foreach (var small in smallObjects) {
            if (small == null) {
                continue;
            }

            if (string.IsNullOrWhiteSpace(small.persistID)) {
                Debug.LogWarning($"SaveManager: {small.name} has no persistID, skipped.");
                continue;
            }

            var data = new SmallObjectSaveData {
                id = small.persistID,
                staticDataName = small.staticData != null ? small.staticData.name : string.Empty,
                objectType = small.GetType().Name,
                sceneName = package.sceneName,
                position = small.transform.position,
                rotation = small.transform.rotation,
                isActive = small.gameObject.activeSelf,
                isDestroyed = small.dynamicState != null && small.dynamicState.isDestroyed,
                properties = new List<PropertySaveEntry>(),
                labels = SaveLabels(small.dynamicState != null ? small.dynamicState.smallObjectLabels : null)
            };

            if (small.dynamicState != null && small.dynamicState.propertyMap != null) {
                foreach (var pair in small.dynamicState.propertyMap) {
                    data.properties.Add(new PropertySaveEntry { name = pair.Key, value = pair.Value.value });
                }
            }

            if (small is PlayerSmallObject player) {
                data.player = new PlayerSaveData {
                    lastAttackTime = player.playerState.lastAttackTime,
                    isRunning = false,                     // 强制：不保存速度
                    facingDirection = player.playerState.facingDirection,
                    labelBackpack = SaveLabels(player.playerState.labelBackpack),
                    itemBackpack = SaveItems(player.playerState.itemBackpack),
                    itemBackpackLegacy = SaveItemNames(player.playerState.itemBackpack)
                };
            }

            if (small is MonsterSmallObject monster) {
                data.monster = new MonsterSaveData {
                    lastAttackTime = monster.monsterState.lastAttackTime,
                    currentOrientation = monster.monsterState.currentOrientation
                };
            }

            if (small is NPCObject npc) {
                data.npc = new NPCSaveData {
                    isInConversation = npc.IsInConversation(),
                    currentNodeIndex = npc.GetCurrentNodeIndex()
                };
            }

            if (small is DoorSmallObject door) {
                data.door = new DoorSaveData {
                    isOpen = door.doorState != null && door.doorState.isOpen
                };
            }

            package.objectStates.Add(data);
        }

        // 保存第三章线索触发进度
        if (Chapter3_SceneController.Instance != null) {
            package.chapter3ClueData = Chapter3_SceneController.Instance.GetClueSaveData();
        }

        return package;
    }

    private void ApplySavePackage(GameSavePackage package) {
        if (package == null) {
            return;
        }

        if (GameSceneManager.Instance != null) {
            GameSceneManager.Instance.SetTimeVision(package.isPresentTime);
        }

        DialogueNodeActionSOActions.ApplyTriggeredTutorialIds(package.triggeredTutorialIds);
        DialogueNodeActionSOActions.ApplyVisibleTutorialIndices(package.visibleTutorialIndices);

        var smallObjects = FindAllSmallObjects();
        var map = new Dictionary<string, SmallObject>(StringComparer.Ordinal);
        foreach (var small in smallObjects) {
            if (small == null || string.IsNullOrWhiteSpace(small.persistID)) {
                continue;
            }
            map[small.persistID] = small;
        }

        foreach (var saved in package.objectStates) {
            if (saved == null || string.IsNullOrWhiteSpace(saved.id)) {
                continue;
            }
            if (!map.TryGetValue(saved.id, out var target)) {
                Debug.LogWarning($"SaveManager: object with id {saved.id} not found in scene.");
                continue;
            }

            ApplySmallObjectSave(target, saved);
        }

        // 恢复第三章线索触发进度
        if (package.chapter3ClueData != null && Chapter3_SceneController.Instance != null) {
            Chapter3_SceneController.Instance.ApplyClueSaveData(package.chapter3ClueData);
        }
    }

    private void ApplySmallObjectSave(SmallObject target, SmallObjectSaveData saved) {
        if (target == null || saved == null) {
            return;
        }

        target.transform.position = saved.position;
        target.transform.rotation = saved.rotation;

        bool shouldBeActive = saved.isActive && !saved.isDestroyed;
        target.gameObject.SetActive(shouldBeActive);

        if (target.dynamicState == null) {
            Debug.LogWarning($"SaveManager: {target.name} has no dynamicState; skipping state restore.");
            return;
        }

        target.dynamicState.isDestroyed = saved.isDestroyed;

        if (saved.properties != null && target.dynamicState.propertyMap != null) {
            foreach (var entry in saved.properties) {
                if (entry == null) {
                    continue;
                }
                if (target.dynamicState.propertyMap.TryGetValue(entry.name, out var prop)) {
                    prop.SetValueWithoutNotify(entry.value);
                }
            }
        }

        ApplyLabels(target, saved.labels);

        if (target is PlayerSmallObject player && saved.player != null) {
            ApplyPlayerSave(player, saved.player);
        }

        if (target is MonsterSmallObject monster && saved.monster != null) {
            monster.monsterState.lastAttackTime = saved.monster.lastAttackTime;
            monster.monsterState.currentOrientation = saved.monster.currentOrientation;
            monster.monsterState.isPlayerInAttackRange = false;
            monster.monsterState.targetPlayer = null;
            if (monster.monsterView != null) {
                Vector2 dir = OrientationToVector(saved.monster.currentOrientation);
                monster.monsterView.UpdateMovement(dir, false);
            }
        }

        if (target is NPCObject npc && saved.npc != null) {
            npc.ApplySaveState(saved.npc.isInConversation, saved.npc.currentNodeIndex, IOSubsystem.Instance?.playerObject);
        }

        if (target is DoorSmallObject door && saved.door != null) {
            if (door.doorState != null) {
                door.doorState.isOpen = saved.door.isOpen;
            }
            door.RefreshLockCollider();
        }
    }

    private void ApplyPlayerSave(PlayerSmallObject player, PlayerSaveData data) {
        if (player == null || data == null) {
            return;
        }

        player.playerState.lastAttackTime = data.lastAttackTime;
        player.playerState.isRunning = false;              // 强制：加载时不恢复速度
        player.playerState.facingDirection = data.facingDirection;

        if (player.playerState.labelBackpack == null) {
            player.playerState.labelBackpack = new List<ObjectLabel>();
        }
        player.playerState.labelBackpack.Clear();
        foreach (var labelData in data.labelBackpack) {
            var label = BuildLabel(labelData);
            if (label != null) {
                player.playerState.labelBackpack.Add(label);
            }
        }

        if (player.playerState.itemBackpack == null) {
            player.playerState.itemBackpack = new List<Item>();
        }
        player.playerState.itemBackpack.Clear();
        if (data.itemBackpack != null && data.itemBackpack.Count > 0) {
            foreach (var itemData in data.itemBackpack) {
                if (itemData == null || string.IsNullOrWhiteSpace(itemData.itemName)) {
                    continue;
                }

                player.playerState.itemBackpack.Add(Item.Create(itemData.itemName, itemData.itemType, itemData.recoverAmount));
            }
        } else if (data.itemBackpackLegacy != null && data.itemBackpackLegacy.Count > 0) {
            foreach (var itemName in data.itemBackpackLegacy) {
                if (string.IsNullOrWhiteSpace(itemName)) {
                    continue;
                }
                player.playerState.itemBackpack.Add(new Item { itemName = itemName });
            }
        }

        Vector2 dir = OrientationToVector(data.facingDirection);
        player.UpdateOrientation(dir);
        if (player.playerView != null) {
            player.playerView.UpdateMovement(dir, false);  // 强制：加载后不奔跑
        }
    }

    private List<ItemSaveData> SaveItems(List<Item> items) {
        var result = new List<ItemSaveData>();
        if (items == null) {
            return result;
        }

        foreach (var item in items) {
            if (item == null || string.IsNullOrWhiteSpace(item.itemName)) {
                continue;
            }

            var itemType = item.itemType;
            float recoverAmount = 0f;
            if (item is RecoverItem recoverItem) {
                recoverAmount = recoverItem.recoverAmount;
                if (itemType != ItemType.Recover) {
                    itemType = ItemType.Recover;
                }
            }

            result.Add(new ItemSaveData {
                itemName = item.itemName,
                itemType = itemType,
                recoverAmount = recoverAmount
            });
        }

        return result;
    }

    private List<string> SaveItemNames(List<Item> items) {
        var result = new List<string>();
        if (items == null) {
            return result;
        }

        foreach (var item in items) {
            if (item == null || string.IsNullOrWhiteSpace(item.itemName)) {
                continue;
            }
            result.Add(item.itemName);
        }

        return result;
    }

    private List<LabelSaveData> SaveLabels(List<ObjectLabel> labels) {
        var result = new List<LabelSaveData>();
        if (labels == null) {
            return result;
        }

        foreach (var label in labels) {
            if (label == null) {
                continue;
            }

            var data = new LabelSaveData {
                labelType = label.GetType().Name
            };

            if (label is LockLabel lockLabel) {
                data.lockIsLocked = lockLabel.IsLocked;
            }

            if (label is GlowLabel glow) {
                data.glowColor = glow.color;
                data.glowIntensity = glow.intensity;
                data.glowRange = glow.range;
                data.glowLightType = (int)glow.lightType;
                data.glowLocalPosition = glow.localPosition;
            }

            if (label is NPCLabelBase npcLabel) {
                // 通过反射获取 overrideConfig 字段
                var field = typeof(NPCLabelBase).GetField("overrideConfig", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) {
                    var config = field.GetValue(npcLabel) as NPCAppearanceOverride;
                    if (config != null) {
                        data.npcOverrideConfigName = config.name;
                    }
                }
            }

            result.Add(data);
        }

        return result;
    }

    private void ApplyLabels(SmallObject target, List<LabelSaveData> labels) {
        if (target == null || labels == null) {
            return;
        }

        if (target.dynamicState == null) {
            target.dynamicState = new SmallObjectDynamicState();
        }

        if (target.dynamicState.smallObjectLabels == null) {
            target.dynamicState.smallObjectLabels = new List<ObjectLabel>();
        } else {
            var existing = new List<ObjectLabel>(target.dynamicState.smallObjectLabels);
            foreach (var label in existing) {
                label?.Detach();
            }
            target.dynamicState.smallObjectLabels.Clear();
        }

        foreach (var labelData in labels) {
            var label = BuildLabel(labelData);
            if (label == null) {
                continue;
            }

            target.dynamicState.smallObjectLabels.Add(label);
            label.AttachToOwner(target);
        }
    }

    private ObjectLabel BuildLabel(LabelSaveData data) {
        if (data == null || string.IsNullOrWhiteSpace(data.labelType)) {
            return null;
        }

        try {
            var label = LabelFactory.Build(data.labelType);
            if (label is LockLabel lockLabel && !data.lockIsLocked) {
                lockLabel.Unlock();
            }
            if (label is GlowLabel glow) {
                glow.color = data.glowColor;
                glow.intensity = data.glowIntensity;
                glow.range = data.glowRange;
                glow.lightType = (LightType)data.glowLightType;
                glow.localPosition = data.glowLocalPosition;
            }
            if (label is NPCLabelBase npcLabel) {
                // 根据保存的配置名称加载 NPCAppearanceOverride asset
                if (!string.IsNullOrWhiteSpace(data.npcOverrideConfigName)) {
                    var config = Resources.Load<NPCAppearanceOverride>($"NPCAppearanceOverrides/{data.npcOverrideConfigName}");
                    if (config != null) {
                        // 通过反射设置 overrideConfig 字段
                        var field = typeof(NPCLabelBase).GetField("overrideConfig", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null) {
                            field.SetValue(npcLabel, config);
                        }
                    } else {
                        Debug.LogWarning($"SaveManager: failed to load NPCAppearanceOverride asset: {data.npcOverrideConfigName}");
                    }
                }
            }
            return label;
        } catch (Exception ex) {
            Debug.LogWarning($"SaveManager: failed to build label {data.labelType}: {ex.Message}");
            return null;
        }
    }

    private Vector2 OrientationToVector(Orientation orientation) {
        switch (orientation) {
            case Orientation.Up:
                return Vector2.up;
            case Orientation.Down:
                return Vector2.down;
            case Orientation.Left:
                return Vector2.left;
            case Orientation.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    private SmallObject[] FindAllSmallObjects() {
#if UNITY_2022_2_OR_NEWER
        return FindObjectsByType<SmallObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var all = Resources.FindObjectsOfTypeAll<SmallObject>();
        var result = new List<SmallObject>();
        foreach (var obj in all) {
            if (obj == null) {
                continue;
            }
            if (!obj.gameObject.scene.IsValid()) {
                continue;
            }
            if ((obj.hideFlags & HideFlags.HideAndDontSave) != 0) {
                continue;
            }
            result.Add(obj);
        }
        return result.ToArray();
#endif
    }
}
