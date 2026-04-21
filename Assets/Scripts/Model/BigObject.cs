using UnityEngine;

public abstract class BigObject : MonoBehaviour {
    public SmallObject pastObject;
    public SmallObject presentObject;

    // 核心逻辑：定义过去如何影响现在
    public abstract void OnChildStateChanged(SmallObject changedChild);

    public virtual void Start() {
        if(pastObject != null) pastObject.ownerBigObject = this;
        if(presentObject != null) presentObject.ownerBigObject = this;
    
    }
}