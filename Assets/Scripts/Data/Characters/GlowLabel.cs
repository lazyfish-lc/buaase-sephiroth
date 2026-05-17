using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GlowLabel : ObjectLabel {
    // 在 Inspector 中可配置的发光参数
    public Color color = Color.white;
    public float intensity = 1f;
    public float range = 5f;
    public LightType lightType = LightType.Point;
    public Vector3 localPosition = Vector3.zero;

    [System.NonSerialized]
    private GameObject glowObject;

    // 使用中文名称以便在编辑器中更直观
    public override string labelName {
        get { return "Glow"; }
    }

    public override void OnAttach(ILabelOwner owner) {
        base.OnAttach(owner);

        var comp = owner as Component;
        if (comp == null) return;

        if (glowObject != null) {
            if (Application.isPlaying) Object.Destroy(glowObject);
            else Object.DestroyImmediate(glowObject);
            glowObject = null;
        }

        // 优先使用序列化引用（场景中的 GlowLabelRegistry）
        GameObject prefab = GlowLabelRegistry.Instance != null
            ? GlowLabelRegistry.Instance.spotLightPrefab
            : null;

        // 兜底：若未配置，则尝试从 Resources 加载
        if (prefab == null) {
            prefab = Resources.Load<GameObject>("SpotLight");
        }

        if (prefab != null) {
            glowObject = Object.Instantiate(prefab, comp.transform);
            glowObject.name = "GlowLight";
            glowObject.transform.localPosition = localPosition;
        } else {
            // 回退：创建普通空对象，避免空引用
            glowObject = new GameObject("GlowLight");
            glowObject.transform.SetParent(comp.transform, false);
            glowObject.transform.localPosition = localPosition;
            Debug.LogWarning("GlowLabel: 未找到 SpotLight 预制体，已创建空对象作为后备。");
        }
    }

    public override void OnDetach(ILabelOwner owner) {
        // 销毁子物体
        if (glowObject != null) {
            if (Application.isPlaying) Object.Destroy(glowObject);
            else Object.DestroyImmediate(glowObject);
            glowObject = null;
        }

        base.OnDetach(owner);
    }
}
