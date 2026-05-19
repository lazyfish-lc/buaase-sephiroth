using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialContentTable", menuName = "Game/Dialogue/Tutorial Content Table")]
public class TutorialContentTable : ScriptableObject {
    public List<TutorialContent> tutorials = new List<TutorialContent>();
}
