using System;
using UnityEngine;

using System.Collections.Generic;
[CreateAssetMenu(fileName = "ItemDisplayTable", menuName = "Game/ItemDisplayTable")]
public class ItemDisplayInfo : ScriptableObject
{
    public List<ItemInfo> itemInfos = new List<ItemInfo>();

}
