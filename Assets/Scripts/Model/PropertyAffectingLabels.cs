using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HardLabel : ObjectLabel, ILabelAffectsProperty {
    private float DEFDelta = 10f;
    public HardLabel() {
    }

    // 自定义类名显示：同类所有实例共享该名称
    public override string labelName => "Hard";

    public IEnumerable<string> GetAffectedPropertyNames() {
        yield return "DEF";
    }

    public float GetPropertyDelta(string propertyName) {
        if (string.Equals(propertyName, "DEF", StringComparison.OrdinalIgnoreCase)) return DEFDelta;
        return 0f;
    }
}

[Serializable]
public class FragileLabel : ObjectLabel, ILabelAffectsProperty {
    private float DEFDelta = -10f;
    public FragileLabel() {
    }

    // 自定义类名显示：同类所有实例共享该名称
    public override string labelName => "Fragile";

    public IEnumerable<string> GetAffectedPropertyNames() {
        yield return "DEF";
    }

    public float GetPropertyDelta(string propertyName) {
        if (string.Equals(propertyName, "DEF", StringComparison.OrdinalIgnoreCase)) return DEFDelta;
        return 0f;
    }
}

[Serializable]
public class DocileLabel : ObjectLabel, ILabelOnTick {
    public override string labelName => "Docile";

    public void ILabelOnTick() {
        var dog = owner as DogObject;
        if (dog == null) return;

        var player = dog.CurrentPlayer;
        if (player == null) return;

        float luck = GetPlayerLuck(player);
        if (luck < dog.LuckThreshold) {
            dog.ReplaceLabel(this, new BerserkLabel());
        }
    }

    private float GetPlayerLuck(PlayerSmallObject player) {
        if (player == null || player.playerState == null) {
            return 0f;
        }

        if (player.playerState.propertyMap != null && player.playerState.propertyMap.TryGetValue("Luck", out var luckProp)) {
            return luckProp.value;
        }

        return 0f;
    }
}

[Serializable]
public class BerserkLabel : ObjectLabel, ILabelOnTick {
    public override string labelName => "Berserk";

    public void ILabelOnTick() {
        var dog = owner as DogObject;
        if (dog == null) return;

        var player = dog.CurrentPlayer;
        if (player == null) return;

        float luck = GetPlayerLuck(player);
        if (luck > dog.LuckThreshold) {
            dog.ReplaceLabel(this, new DocileLabel());
        }
    }

    private float GetPlayerLuck(PlayerSmallObject player) {
        if (player == null || player.playerState == null) {
            return 0f;
        }

        if (player.playerState.propertyMap != null && player.playerState.propertyMap.TryGetValue("Luck", out var luckProp)) {
            return luckProp.value;
        }

        return 0f;
    }
}
