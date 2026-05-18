using UnityEngine;

using System.Collections.Generic;

public abstract class NumericalRule {
    public abstract IReadOnlyList<SmallObjectProperty> GetManagedProperties();

    public abstract bool CheckValid(NumericalModificationRequest request);

    public abstract void Apply(NumericalModificationRequest request);
}