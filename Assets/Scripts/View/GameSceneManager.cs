using UnityEngine;
using System;

public enum TimePeriod {
    Present,
    Past
}

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Map Roots")]
    public GameObject pastMapRoot;    // “过去”地图的根节点
    public GameObject presentMapRoot; // “现在”地图的根节点

    [Header("State")]
    public TimePeriod currentTimePeriod = TimePeriod.Present;

    [Header("Player Reference")]
    public Transform playerTransform;

    public event Action<TimePeriod> OnMapSwitched;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 游戏启动时默认进入“现在”
        InitializeMaps();
    }

    private void InitializeMaps()
    {
        // 确保逻辑一致性
        if (currentTimePeriod == TimePeriod.Present)
        {
            presentMapRoot.SetActive(true);
            pastMapRoot.SetActive(false);
        }
        else
        {
            presentMapRoot.SetActive(false);
            pastMapRoot.SetActive(true);
        }
    }

    /// <summary>
    /// 执行地图级切换
    /// </summary>
    public void ToggleTimeVision()
    {
        // 记录切换前状态
        TimePeriod targetPeriod = (currentTimePeriod == TimePeriod.Present) 
                                  ? TimePeriod.Past 
                                  : TimePeriod.Present;

        // --- View 层操作：切换地图显示 ---
        if (targetPeriod == TimePeriod.Past)
        {
            presentMapRoot.SetActive(false);
            pastMapRoot.SetActive(true);
        }
        else
        {
            pastMapRoot.SetActive(false);
            presentMapRoot.SetActive(true);
        }

        // --- Model 层操作：更新状态 ---
        currentTimePeriod = targetPeriod;

        // --- 效果增强：可以在这里添加转场动画或特效 ---
        PlayTransitionEffect();

        Debug.Log($"地图已切换至: {currentTimePeriod}");
        OnMapSwitched?.Invoke(currentTimePeriod);
    }

    private void PlayTransitionEffect()
    {
        // 示例：简单的屏幕闪烁或粒子效果
        Debug.Log("播放时空穿梭特效...");
    }

    // 此时判断是否可见变得非常简单：只要对象是激活的，就是可见的
    public bool IsInCurrentTime(SmallObject obj)
    {
        return obj.gameObject.activeInHierarchy;
    }
}