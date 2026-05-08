using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LuckyOptionRule {
    public int optionIndex;
    public int luckCost;
    public int rewardAmount;
}

public class LuckyChoiceNPCObject : NPCObject {
    [SerializeField] private string luckPropertyName = "Luck";
    [SerializeField] private string rewardItemName = "Luck";
    [SerializeField] private int insufficientNodeIndex = -1;
    [SerializeField] private int exchangeNodeIndex = -1;
    [SerializeField] private List<LuckyOptionRule> optionRules = new List<LuckyOptionRule>();

    private void Reset() {
        optionRules = new List<LuckyOptionRule> {
            new LuckyOptionRule { optionIndex = 0, luckCost = 100, rewardAmount = 20 },
            new LuckyOptionRule { optionIndex = 1, luckCost = 50, rewardAmount = 10 },
            new LuckyOptionRule { optionIndex = 2, luckCost = 20, rewardAmount = 4 }
        };
    }

    public override void SelectOption(int optionIndex) {
        if (!IsInConversation()) return;

        if (exchangeNodeIndex >= 0 && GetCurrentNodeIndex() != exchangeNodeIndex) {
            base.SelectOption(optionIndex);
            return;
        }

        if (!TryGetRule(optionIndex, out var rule)) {
            base.SelectOption(optionIndex);
            return;
        }

        var player = CurrentInteractingPlayer;
        if (player == null || player.playerState == null) {
            TransitionToInsufficientNode();
            return;
        }

        if (!TryGetLuckProperty(player, out var luckProp, out var luckValue)) {
            TransitionToInsufficientNode();
            return;
        }

        if (luckValue < rule.luckCost) {
            TransitionToInsufficientNode();
            return;
        }

        float newLuck = luckValue - rule.luckCost;
        luckProp.value = newLuck;
        Debug.Log($"玩家 {player.name} 选择了选项 {optionIndex}，消耗了 {rule.luckCost} 点幸运值，剩余幸运值 {newLuck}");

        GiveLuckItems(player, rule.rewardAmount);

        var options = GetCurrentOptions();
        if (options == null || optionIndex < 0 || optionIndex >= options.Count) {
            return;
        }

        TransitionToNode(options[optionIndex].targetNodeIndex);
    }

    private bool TryGetRule(int optionIndex, out LuckyOptionRule rule) {
        for (int i = 0; i < optionRules.Count; i++) {
            if (optionRules[i].optionIndex == optionIndex) {
                rule = optionRules[i];
                return true;
            }
        }

        rule = default;
        return false;
    }

    private bool TryGetLuckProperty(PlayerSmallObject player, out SmallObjectProperty luckProp, out float luckValue) {
        luckProp = null;
        luckValue = 0f;

        if (player.playerState.propertyMap == null) {
            return false;
        }

        if (!player.playerState.propertyMap.TryGetValue(luckPropertyName, out luckProp)) {
            return false;
        }

        luckValue = luckProp.value;
        return true;
    }

    private void GiveLuckItems(PlayerSmallObject player, int amount) {
        if (player == null || amount <= 0 || string.IsNullOrWhiteSpace(rewardItemName)) {
            return;
        }

        for (int i = 0; i < amount; i++) {
            player.AddItemToBackpack(new Item { itemName = rewardItemName });
        }
    }

    private void TransitionToInsufficientNode() {
        if (insufficientNodeIndex < 0) {
            Debug.LogWarning("LuckyChoiceNPCObject: insufficientNodeIndex not set.");
            TransitionToNode(-1);
            return;
        }

        TransitionToNode(insufficientNodeIndex);
    }
}
