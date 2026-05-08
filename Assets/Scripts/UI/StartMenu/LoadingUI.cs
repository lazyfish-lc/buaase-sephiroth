using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance;

    [Header("UI 组件")]
    public GameObject loadingCanvas;      // 整个加载界面根物体
    public Slider progressBar;           // 可选进度条
    public TMP_Text loadingText;             // 可选文字提示

    [Header("设置")]
    public float fakeLoadDuration = 1f;  // 假进度持续时长（秒）

    public static bool isLoading = false;

    private Coroutine dotsCoroutine;
    private string loadingBaseText = "加载中";
    private readonly WaitForSeconds dotsInterval = new WaitForSeconds(1f / 6f); // 一秒六个点

    void Awake()
    {
        // 单例实现 + 跨场景保留
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 关键：让这个物体不随场景销毁
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始状态下隐藏加载界面
        if (loadingCanvas != null)
            loadingCanvas.SetActive(false);
    }

    /// <summary>
    /// 显示加载界面（不开始加载，仅显示）
    /// </summary>
    public void Show()
    {
        if (loadingCanvas != null)
            loadingCanvas.SetActive(true);
        if (loadingText != null)
            loadingText.text = loadingBaseText; // 初始瞬间显示"加载中"
        if (dotsCoroutine == null)
            dotsCoroutine = StartCoroutine(DotsAnimationCoroutine());
    }

    /// <summary>
    /// 隐藏加载界面
    /// </summary>
    public void Hide()
    {
        if (dotsCoroutine != null)
        {
            StopCoroutine(dotsCoroutine);
            dotsCoroutine = null;
        }
        if (loadingCanvas != null)
            loadingCanvas.SetActive(false);
    }

    private IEnumerator DotsAnimationCoroutine()
    {
        if (loadingText == null) yield break;

        loadingBaseText = loadingText.text.TrimEnd('.');

        int dotCount = 0;
        while (true)
        {
            loadingText.text = loadingBaseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 7; // 0→6 循环
            yield return dotsInterval;
        }
    }

    /// <summary>
    /// 切换场景并显示加载界面（带最短时间控制）
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        AudioManager.Instance.Stop(); // 切场景前停止所有音乐和音效
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        isLoading = true;
        Show();

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        float startTime = Time.time;

        // 阶段1: 一秒内增加到50%
        while (Time.time - startTime < fakeLoadDuration)
        {
            float t = (Time.time - startTime) / fakeLoadDuration;
            float fakeProgress = 0.5f * t * t * t; // 加速
            if (progressBar != null)
                progressBar.value = fakeProgress;
            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 0.5f;

        // 阶段2: 检查实际加载进度
        if (async.progress >= 0.9f)
        {
            // 快于一秒：瞬间完成
            if (progressBar != null)
                progressBar.value = 1f;
        }
        else
        {
            // 慢于一秒：显示真实进度 50%→90%
            while (async.progress < 0.9f)
            {
                float displayProgress = 0.5f + async.progress * (0.9f - 0.5f) / 0.9f;
                if (progressBar != null)
                    progressBar.value = displayProgress;
                yield return null;
            }

            if (progressBar != null)
                progressBar.value = 0.9f;
        }

        async.allowSceneActivation = true;

        // 注意：新场景激活后，当前 GameObject 依然存在（因为 DontDestroyOnLoad）
        // 但我们需要在新场景完全加载后自动隐藏加载界面
        // 方法1：等待一帧，让新场景的 Start 执行
        yield return null;
        Hide();
        isLoading = false;
    }
}