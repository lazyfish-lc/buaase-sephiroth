using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class PlayerSceneStateCache {
    private static readonly Dictionary<string, float> cachedProperties = new Dictionary<string, float>();
    private static readonly List<ItemSaveData> cachedItems = new List<ItemSaveData>();
    private static readonly List<LabelSaveData> cachedBackpackLabels = new List<LabelSaveData>();
    private static readonly List<LabelSaveData> cachedObjectLabels = new List<LabelSaveData>();
    private static bool hasCache;

    public static bool HasCache => hasCache;

    public static void Capture(PlayerSmallObject player) {
        if (player == null || player.playerState == null || player.playerState.propertyMap == null) {
            return;
        }

        cachedProperties.Clear();
        foreach (var pair in player.playerState.propertyMap) {
            if (pair.Key == null || pair.Value == null) {
                continue;
            }
            cachedProperties[pair.Key] = pair.Value.value;
        }

        CacheItems(player.playerState.itemBackpack);
        CacheLabels(player.playerState.labelBackpack, cachedBackpackLabels);
        CacheLabels(player.dynamicState != null ? player.dynamicState.smallObjectLabels : null, cachedObjectLabels);

        hasCache = true;
    }

    public static void Apply(PlayerSmallObject player) {
        if (!hasCache || player == null || player.playerState == null || player.playerState.propertyMap == null) {
            return;
        }

        foreach (var pair in cachedProperties) {
            if (player.playerState.propertyMap.TryGetValue(pair.Key, out var prop)) {
                prop.SetValueWithoutNotify(pair.Value);
            }
        }

        ApplyItems(player.playerState);
        ApplyLabelsToBackpack(player.playerState);
        ApplyLabelsToOwner(player);
    }

    public static void Clear() {
        cachedProperties.Clear();
        cachedItems.Clear();
        cachedBackpackLabels.Clear();
        cachedObjectLabels.Clear();
        hasCache = false;
    }

    private static void CacheItems(List<Item> items) {
        cachedItems.Clear();
        if (items == null) {
            return;
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

            cachedItems.Add(new ItemSaveData {
                itemName = item.itemName,
                itemType = itemType,
                recoverAmount = recoverAmount
            });
        }
    }

    private static void ApplyItems(PlayerObjectDynamicState state) {
        if (state == null) {
            return;
        }

        if (state.itemBackpack == null) {
            state.itemBackpack = new List<Item>();
        }
        state.itemBackpack.Clear();
        for (int i = 0; i < cachedItems.Count; i++) {
            var itemData = cachedItems[i];
            if (itemData == null || string.IsNullOrWhiteSpace(itemData.itemName)) {
                continue;
            }
            state.itemBackpack.Add(Item.Create(itemData.itemName, itemData.itemType, itemData.recoverAmount));
        }
    }

    private static void CacheLabels(List<ObjectLabel> labels, List<LabelSaveData> target) {
        target.Clear();
        if (labels == null) {
            return;
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

            target.Add(data);
        }
    }

    private static void ApplyLabelsToBackpack(PlayerObjectDynamicState state) {
        if (state == null) {
            return;
        }

        if (state.labelBackpack == null) {
            state.labelBackpack = new List<ObjectLabel>();
        }
        state.labelBackpack.Clear();

        for (int i = 0; i < cachedBackpackLabels.Count; i++) {
            var label = BuildLabel(cachedBackpackLabels[i]);
            if (label != null) {
                state.labelBackpack.Add(label);
            }
        }
    }

    private static void ApplyLabelsToOwner(PlayerSmallObject player) {
        if (player == null) {
            return;
        }

        if (player.dynamicState == null) {
            player.dynamicState = new SmallObjectDynamicState();
        }

        if (player.dynamicState.smallObjectLabels == null) {
            player.dynamicState.smallObjectLabels = new List<ObjectLabel>();
        } else {
            var existing = new List<ObjectLabel>(player.dynamicState.smallObjectLabels);
            for (int i = 0; i < existing.Count; i++) {
                existing[i]?.Detach();
            }
            player.dynamicState.smallObjectLabels.Clear();
        }

        for (int i = 0; i < cachedObjectLabels.Count; i++) {
            var label = BuildLabel(cachedObjectLabels[i]);
            if (label != null) {
                player.dynamicState.smallObjectLabels.Add(label);
                label.AttachToOwner(player);
            }
        }
    }

    private static ObjectLabel BuildLabel(LabelSaveData data) {
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
                        Debug.LogWarning($"PlayerSceneStateCache: failed to load NPCAppearanceOverride asset: {data.npcOverrideConfigName}");
                    }
                }
            }
            return label;
        } catch (Exception ex) {
            Debug.LogWarning($"PlayerSceneStateCache: failed to build label {data.labelType}: {ex.Message}");
            return null;
        }
    }
}
