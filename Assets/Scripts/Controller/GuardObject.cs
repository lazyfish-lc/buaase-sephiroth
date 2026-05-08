using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class GuardObject : NPCObject, IDialogueActionReceiver {
    [Header("Guard Dialogue")]
    [SerializeField] private string guardActionId;
    [SerializeField] private int passInspectionNodeIndex = 0;

    [Header("Pass Inspection")]
    [SerializeField] private string passItemName = "Pass";
    [SerializeField] private string luckPropertyName = "Luck";
    [SerializeField] private float luckTriggerValue = 0f;
    [SerializeField] private string passBlownAwayMessage = "通行证被风吹走了。";

    [Header("Pass Fly Away Animation")]
    [SerializeField] private float flyAwayDuration = 0.75f;
    [SerializeField] private float flyAwayDistance = 3.0f;
    [SerializeField] private float flyAwayArcHeight = 1.4f;
    [SerializeField] private float flyAwaySpinSpeed = 720f;
    [SerializeField] private float flyAwayScaleMultiplier = 0.35f;
    [SerializeField] private Vector3 flyAwaySpawnOffset = new Vector3(0.15f, 0.65f, 0f);

    public event Action<GuardObject> OnPassBlownAway;

    private bool isBlowingPassAway;

    public string DialogueActionId => !string.IsNullOrWhiteSpace(guardActionId)
        ? guardActionId
        : !string.IsNullOrWhiteSpace(NPCData?.objectName)
            ? NPCData.objectName
            : gameObject.name;

    protected override int ResolveStartNodeIndex() {
        var player = CurrentInteractingPlayer;
        if (player != null && player.HasItemInBackpack(passItemName) && TryGetPlayerLuck(player, out float luck)) {
            if (Mathf.Approximately(luck, luckTriggerValue)) {
                return Mathf.Max(0, passInspectionNodeIndex);
            }
        }

        return base.ResolveStartNodeIndex();
    }

    public void ReceiveDialogueAction() {
        var player = CurrentInteractingPlayer;
        if (player == null) {
            Debug.LogWarning($"{gameObject.name} 执行通行证吹走动作失败：当前没有交互中的玩家");
            return;
        }

        if (isBlowingPassAway) {
            return;
        }

        if (!player.HasItemInBackpack(passItemName)) {
            Debug.LogWarning($"{gameObject.name} 尝试吹走通行证，但玩家背包中没有 {passItemName}");
            return;
        }

        StartCoroutine(PlayPassBlownAwayFlow(player));
    }

    private IEnumerator PlayPassBlownAwayFlow(PlayerSmallObject player) {
        isBlowingPassAway = true;

        GameObject visual = CreatePassVisual();
        if (visual == null) {
            Debug.LogWarning($"{gameObject.name} 无法创建通行证飞行动画对象，直接移除物品");
            player.RemoveItemFromBackpack(passItemName, 1);
            Debug.Log($"{gameObject.name}: {passBlownAwayMessage}");
            OnPassBlownAway?.Invoke(this);
            isBlowingPassAway = false;
            yield break;
        }

        Vector3 startPos = player.transform.position + flyAwaySpawnOffset;
        visual.transform.position = startPos;

        Vector3 awayDirection = GetFlyAwayDirection(player);
        Vector3 endPos = startPos + awayDirection * flyAwayDistance;
        Vector3 controlPos = (startPos + endPos) * 0.5f + Vector3.up * flyAwayArcHeight;

        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;
        float elapsed = 0f;

        while (elapsed < flyAwayDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, flyAwayDuration));
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            visual.transform.position = EvaluateQuadraticBezier(startPos, controlPos, endPos, easedT);
            visual.transform.Rotate(0f, 0f, flyAwaySpinSpeed * Time.deltaTime);

            float scaleT = Mathf.Lerp(1f, flyAwayScaleMultiplier, easedT);
            visual.transform.localScale = Vector3.one * scaleT;

            if (sr != null) {
                sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, easedT));
            }

            yield return null;
        }

        Destroy(visual);

        player.RemoveItemFromBackpack(passItemName, 1);
        Debug.Log($"{gameObject.name}: {passBlownAwayMessage}");
        OnPassBlownAway?.Invoke(this);
        isBlowingPassAway = false;
    }

    private bool TryGetPlayerLuck(PlayerSmallObject player, out float luck) {
        luck = 0f;
        if (player == null) return false;

        if (!player.TryGetPropertyValue(luckPropertyName, out luck, includeLabelAffect: true)) {
            luck = 0f;
        }

        return true;
    }

    private GameObject CreatePassVisual() {
        Sprite sprite = ResolvePassSprite();
        if (sprite == null) {
            return null;
        }

        GameObject visual = new GameObject($"{passItemName}_FlyingVisual");
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        var guardRenderer = GetComponent<SpriteRenderer>();
        if (guardRenderer != null) {
            sr.sortingLayerID = guardRenderer.sortingLayerID;
            sr.sortingOrder = guardRenderer.sortingOrder + 10;
        }

        visual.transform.localScale = Vector3.one;
        return visual;
    }

    private Sprite ResolvePassSprite() {
        if (DisplayTable.Instance != null) {
            var info = DisplayTable.Instance.GetDisplayInfo(passItemName, "Item");
            if (info != null && info.icon != null) {
                return info.icon;
            }
        }

        var npcSprite = GetComponent<SpriteRenderer>()?.sprite;
        if (npcSprite != null) {
            return npcSprite;
        }

        Texture2D tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Vector3 GetFlyAwayDirection(PlayerSmallObject player) {
        Vector3 direction = new Vector3(1f, 0.35f, 0f);
        direction.Normalize();
        return direction;
    }

    private Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}
