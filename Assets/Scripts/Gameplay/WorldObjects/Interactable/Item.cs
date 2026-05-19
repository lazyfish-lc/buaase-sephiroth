[System.Serializable]
public class Item {
    public string itemName;
    public ItemType itemType;

    public static Item Create(string itemName, ItemType itemType = ItemType.Normal, float recoverAmount = 0f) {
        if (itemType == ItemType.Recover) {
            return new RecoverItem {
                itemName = itemName,
                itemType = itemType,
                recoverAmount = recoverAmount
            };
        }

        return new Item {
            itemName = itemName,
            itemType = itemType
        };
    }

    public static Item CreateFrom(Item blueprint) {
        if (blueprint == null) return null;

        if (blueprint.itemType == ItemType.Recover) {
            float recoverAmount = 0f;
            if (blueprint is RecoverItem recoverBlueprint) {
                recoverAmount = recoverBlueprint.recoverAmount;
            }

            return new RecoverItem {
                itemName = blueprint.itemName,
                itemType = blueprint.itemType,
                recoverAmount = recoverAmount
            };
        }

        return new Item {
            itemName = blueprint.itemName,
            itemType = blueprint.itemType
        };
    }
}

public enum ItemType {
    Normal,
    Recover,
}