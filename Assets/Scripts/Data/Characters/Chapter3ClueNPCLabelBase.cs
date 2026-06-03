using System;
using UnityEngine;

/// <summary>
/// 第三章线索标签基类：优先复用 NPCLabelBase 的 NPC 外观/对话覆盖能力，
/// 同时允许在 SmallObject 上作为章节线索标签使用。
/// </summary>
[Serializable]
public abstract class Chapter3ClueNPCLabelBase : NPCLabelBase {
    protected void InitializeOverrideConfig(
        ref NPCAppearanceOverride cachedConfig,
        string resourcesPath,
        string editorAssetPath,
        string labelTypeName
    ) {
        if (cachedConfig == null) {
            cachedConfig = Resources.Load<NPCAppearanceOverride>(resourcesPath);

#if UNITY_EDITOR
            if (cachedConfig == null && !string.IsNullOrWhiteSpace(editorAssetPath)) {
                try {
                    var config = UnityEditor.AssetDatabase.LoadAssetAtPath<NPCAppearanceOverride>(editorAssetPath);
                    if (config != null) {
                        cachedConfig = config;
                    }
                } catch (Exception ex) {
                    Debug.LogWarning($"{labelTypeName}: AssetDatabase load failed: {ex.Message}");
                }
            }
#endif

            if (cachedConfig == null) {
                Debug.LogError(
                    $"{labelTypeName}: Failed to load NPCAppearanceOverride. " +
                    $"Tried Resources('{resourcesPath}')" +
                    (string.IsNullOrWhiteSpace(editorAssetPath) ? string.Empty : $" and Editor path ('{editorAssetPath}')")
                );
            }
        }

        overrideConfig = cachedConfig;
    }

    public override void OnDetach(ILabelOwner owner) {
        base.OnDetach(owner);

        if (owner is SmallObject smallObject) {
            OnDetachFromClueObject(smallObject);
        }
    }

    protected abstract void OnDetachFromClueObject(SmallObject smallObject);
}
