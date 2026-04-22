using UnityEngine;

public abstract class BigObject : MonoBehaviour {
    public BigObjectStaticData staticData;
    public BigObjectDynamicState dynamicState;
    

    // 核心逻辑：定义过去如何影响现在
    public abstract void OnChildStateChanged(SmallObject changedChild);

    public virtual void Start() {
        dynamicState = new BigObjectDynamicState();
        if(staticData.pastObject != null) staticData.pastObject.staticData.ownerBigObject = this;
        if(staticData.presentObject != null) staticData.presentObject.staticData.ownerBigObject = this;
    }
}