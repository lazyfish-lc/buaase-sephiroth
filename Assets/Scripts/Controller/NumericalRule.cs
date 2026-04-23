using UnityEngine;

using System.Collections.Generic;

public class NumericalRule {
    public SmallObjectProperty inputProperty; // 源属性
    public List<SmallObjectProperty> outputProperties = new List<SmallObjectProperty>(); // 目标属性列表

    public virtual bool CheckValid(float oldValue, float newValue) {
        return true;
    }

    public virtual void Notify(float oldValue, float newValue) {
    }
}