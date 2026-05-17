using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NumericalPropertyModification {
    public SmallObjectProperty property;
    public float newValue;

    public NumericalPropertyModification() {
    }

    public NumericalPropertyModification(SmallObjectProperty property, float newValue) {
        this.property = property;
        this.newValue = newValue;
    }
}

[System.Serializable]
public class NumericalModificationRequest {
    private readonly List<NumericalPropertyModification> modifications = new List<NumericalPropertyModification>();

    public IReadOnlyList<NumericalPropertyModification> Modifications => modifications;

    public int Count => modifications.Count;

    public bool AddModification(SmallObjectProperty property, float newValue) {
        if (property == null) {
            Debug.LogWarning("修改请求添加失败：属性为空");
            return false;
        }

        if (ContainsProperty(property)) {
            Debug.LogWarning($"修改请求添加失败：属性 {property.name} 已存在，不能重复修改");
            return false;
        }

        modifications.Add(new NumericalPropertyModification(property, newValue));
        return true;
    }

    public bool ContainsProperty(SmallObjectProperty property) {
        if (property == null) {
            return false;
        }

        for (int i = 0; i < modifications.Count; i++) {
            if (ReferenceEquals(modifications[i].property, property)) {
                return true;
            }
        }

        return false;
    }

    public bool TryGetModification(SmallObjectProperty property, out float newValue) {
        if (property != null) {
            for (int i = 0; i < modifications.Count; i++) {
                if (ReferenceEquals(modifications[i].property, property)) {
                    newValue = modifications[i].newValue;
                    return true;
                }
            }
        }

        newValue = default;
        return false;
    }
}