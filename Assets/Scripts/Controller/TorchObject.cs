using UnityEngine;

public class TorchObject : SmallObject {
    public Sprite litSprite;
    public Sprite unlitSprite;

    private SpriteRenderer sr;

    protected override void Awake() {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();

        EnsureGlowLabelExists();
        RefreshSprite();
    }

    public override void Start() {
        base.Start();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        RefreshSprite();
    }

    private void Update() {
        RefreshSprite();
    }

    private bool HasGlowLabel() {
        return FindGlowLabel() != null;
    }

    private GlowLabel FindGlowLabel() {
        if (dynamicState == null || dynamicState.smallObjectLabels == null) return null;

        foreach (var label in dynamicState.smallObjectLabels) {
            if (label is GlowLabel glow) {
                return glow;
            }
        }

        return null;
    }

    private void EnsureGlowLabelExists() {
        if (FindGlowLabel() != null) return;
        AddLabel(new GlowLabel());
    }

    private void RefreshSprite() {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sprite = HasGlowLabel() ? litSprite : unlitSprite;
    }

    // 便捷的点灯/熄灭接口，供其他系统调用
    public void LightUp() {
        EnsureGlowLabelExists();
        RefreshSprite();
    }

    public void Extinguish() {
        var label = FindGlowLabel();
        if (label != null) {
            RemoveLabel(label);
        }

        RefreshSprite();
    }

    // 交互时切换状态（可按需修改）
    public override void OnInteractAction(InputEventData data) {
        if (HasGlowLabel()) Extinguish();
        else LightUp();
    }
}
