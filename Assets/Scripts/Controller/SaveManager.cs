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

    private GameSavePackage pendingLoad;
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
            SceneManager.sceneLoaded += OnSceneLoaded;
            FindFirstObjectByType<LoadingUI>().LoadScene(package.sceneName);
            return;
        }

        isLoadingSave = true;
        ApplySavePackage(package);
        ScheduleClearLoadingFlag();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (pendingLoad == null) {
            isLoadingSave = false;
            return;
        }

        ApplySavePackage(pendingLoad);
        pendingLoad = null;
        ScheduleClearLoadingFlag();
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
            objectStates = new List<SmallObjectSaveData>()
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
                    isRunning = player.playerState.isRunning,
                    facingDirection = player.playerState.facingDirection,
                    labelBackpack = SaveLabels(player.playerState.labelBackpack),
                    itemBackpack = SaveItems(player.playerState.itemBackpack)
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

        return package;
    }

    private void ApplySavePackage(GameSavePackage package) {
        if (package == null) {
            return;
        }

        if (GameSceneManager.Instance != null) {
            GameSceneManager.Instance.SetTimeVision(package.isPresentTime);
        }

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
        player.playerState.isRunning = data.isRunning;
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
        foreach (var itemName in data.itemBackpack) {
            if (string.IsNullOrWhiteSpace(itemName)) {
                continue;
            }
            player.playerState.itemBackpack.Add(new Item { itemName = itemName });
        }

        Vector2 dir = OrientationToVector(data.facingDirection);
        player.UpdateOrientation(dir);
        if (player.playerView != null) {
            player.playerView.UpdateMovement(dir, data.isRunning);
        }
    }

    private List<string> SaveItems(List<Item> items) {
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
