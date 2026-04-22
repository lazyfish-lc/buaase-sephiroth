using UnityEngine;

public class EnvironmentBigObject : BigObject 
{
    public override void OnChildStateChanged(SmallObject changedChild) 
    {
        // 逻辑不受地图禁用影响
        if (changedChild == pastObject) 
        {
            Debug.Log("BigObject 捕获到过去地图中的状态变化");
            
            // 玩法3：过去影响现在
            if (((TreeSmallObject) pastObject).isDestroyed) 
            {
                // 即使 presentObject (门) 所在的地图当前是禁用的
                // 我们依然可以修改它的数据状态
                if (presentObject is DoorSmallObject door) 
                {
                    ((DoorSmallObject) door).isDestroyed = true; 
                    // 当玩家切换回“现在”地图时，该对象会在 Start 或 OnEnable 中表现为消失
                    door.gameObject.SetActive(false); 
                }
            }
        }
    }
}