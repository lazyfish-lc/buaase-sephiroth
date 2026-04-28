using System;
using UnityEngine;

using System.Collections.Generic;
[CreateAssetMenu(fileName = "ItemDisplayTable", menuName = "Game/ItemDisplayTable")]
public class ItemDisplayTable : ScriptableObject
{
    public List<ItemInfo> itemInfos = new List<ItemInfo>();

}
