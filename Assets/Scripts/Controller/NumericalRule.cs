using UnityEngine;

using System.Collections.Generic;

public class NumericalRule {
    public SmallObjectProperty inputProperty; // 源属性
    public List<SmallObjectProperty> outputProperties = new List<SmallObjectProperty>(); // 目标属性列表

    public void Notify(float oldValue, float newValue) {
        // TODO: 根据输入属性的变化，计算输出属性的新值,并调用OnChangedWithoutNotify方法进行修改
    }
}