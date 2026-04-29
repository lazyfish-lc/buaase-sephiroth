using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SmallObjectDisplayTable", menuName = "Game/SmallObjectDisplayTable")]
public class SmallObjectDisplayTable : ScriptableObject
{
    public List<SmallObjectInfo> smallObjectInfos = new List<SmallObjectInfo>();

}
