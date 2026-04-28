using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LabelDisplayTable", menuName = "Game/LabelDisplayTable")]
public class LabelDisplayTable : ScriptableObject
{
    public List<LabelInfo> labelInfos = new List<LabelInfo>();

}
