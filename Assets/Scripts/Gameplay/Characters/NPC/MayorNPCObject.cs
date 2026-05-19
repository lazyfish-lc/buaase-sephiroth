using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class MayorNPCObject : NPCObject {
    [SerializeField] private string luckCoinItemName = "LuckCoin";
    [SerializeField] private string passportItemName = "PassChapter2";
    [SerializeField] private int docileLabelCount = 30;
    [SerializeField] private int exchangeOptionIndex = -1; // 交换物品选项的索引
    [SerializeField] private int successNodeIndex = -1; // 成功的结束对话A的索引
    [SerializeField] private int failureNodeIndex = -1; // 失败的结束对话B的索引
    [SerializeField] private int choiceNodeIndex = -1; // 选择交换物品的对话节点索引

    public override void SelectOption(int optionIndex) {
        if (!IsInConversation()) return;

        if (choiceNodeIndex >= 0 && GetCurrentNodeIndex() != choiceNodeIndex) {
            base.SelectOption(optionIndex);
            return;
        }

        // 只有选择交换物品选项时才进行检查和交易
        if (exchangeOptionIndex >= 0 && optionIndex != exchangeOptionIndex) {
            base.SelectOption(optionIndex);
            return;
        }

        // 如果设置了exchangeOptionIndex但选项不匹配，则结束
        if (exchangeOptionIndex < 0) {
            base.SelectOption(optionIndex);
            return;
        }

        var player = CurrentInteractingPlayer;
        if (player == null || player.playerState == null) {
            TransitionToNode(failureNodeIndex);
            return;
        }

        // 检查玩家背包中是否有LuckCoin
        int luckCoinCount = CountItemInBackpack(player, luckCoinItemName);
        Debug.Log($"玩家 {player.name} 背包中有 {luckCoinCount} 个 {luckCoinItemName}，需要 {docileLabelCount} 个进行交换");
        if (luckCoinCount < docileLabelCount) {
            // 没有足够的LuckCoin，进入结束对话B
            Debug.Log($"玩家 {player.name} 没有足够的 {luckCoinItemName} 进行交换，当前数量: {luckCoinCount}");
            TransitionToNode(failureNodeIndex);
            return;
        }

        // 消耗所有LuckCoin
        if (!player.RemoveItemFromBackpack(luckCoinItemName, luckCoinCount)) {
            TransitionToNode(failureNodeIndex);
            return;
        }

        Debug.Log($"玩家 {player.name} 交易成功，消耗了 {luckCoinCount} 个 {luckCoinItemName}");

        // 给予玩家1个通行证
        player.AddItemToBackpack(Item.Create(passportItemName));

        // 给予玩家2个温顺标签
        //for (int i = 0; i < 2; i++) {
            //player.playerState.labelBackpack.Add(new DocileLabel());
        //}

        Debug.Log($"玩家 {player.name} 获得了1个{passportItemName}和2个温顺标签");

        // 进入结束对话A
        TransitionToNode(successNodeIndex);
    }

    private int CountItemInBackpack(PlayerSmallObject player, string itemName) {
        if (player == null || player.playerState == null || player.playerState.itemBackpack == null) {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(itemName)) {
            return 0;
        }

        int count = 0;
        foreach (var item in player.playerState.itemBackpack) {
            if (item != null && string.Equals(item.itemName, itemName, System.StringComparison.Ordinal)) {
                count++;
            }
        }

        return count;
    }
}
