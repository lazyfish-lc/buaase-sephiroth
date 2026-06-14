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
    public Image circleOverlay;          // 圆形遮罩（全屏Image，无须手动挂材质）

    [Header("设置")]
    public float fakeLoadDuration = 1f;  // 假进度持续时长（秒）
    public float circleTransitionDuration = 1f; // 转场时长（秒）

    public static bool isLoading = false;

    private Coroutine dotsCoroutine;
    private string loadingBaseText = "加载中";
    private readonly WaitForSeconds dotsInterval = new WaitForSeconds(1f / 6f);
    private Material circleMaterial;

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

        if (circleOverlay != null)
        {
            Shader shader = Shader.Find("UI/CircleReveal");
            if (shader != null)
            {
                circleMaterial = new Material(shader);
                circleOverlay.material = circleMaterial;
            }
            else
            {
                Debug.LogWarning("LoadingUI: 未找到 Shader 'UI/CircleReveal'，圆形转场将不可用");
            }
            circleOverlay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示加载界面（不开始加载，仅显示）
    /// </summary>
    public void Show()
    {
        if (loadingCanvas != null)
            loadingCanvas.SetActive(true);
        if (progressBar != null)
            progressBar.gameObject.SetActive(true);
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = loadingBaseText;
        }
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
        AudioManager.Instance.Stop();

        // 保存当前场景状态，以便返回时恢复（标签、物品等不会丢失）
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCurrentToPending();
        }

        // 同步设置初始状态，下一帧协程直接开始动画，消除延迟
        if (loadingCanvas != null)
            loadingCanvas.SetActive(true);
        if (circleOverlay != null && circleMaterial != null)
        {
            circleOverlay.gameObject.SetActive(true);
            circleMaterial.SetFloat("_Radius", 1f);
            circleMaterial.SetFloat("_AspectRatio", (float)Screen.width / Screen.height);
        }

        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    // 圆形缩小（iris out）：radius 1→0，靠外越快
    private IEnumerator CircleOutCoroutine()
    {
        if (circleMaterial == null || circleOverlay == null) yield break;

        float startTime = Time.time;
        while (Time.time - startTime < circleTransitionDuration)
        {
            float t = (Time.time - startTime) / circleTransitionDuration;
            float val = (1f - t) * (1f - t) * (1f - t); // 三次 ease-out：靠外更快
            circleMaterial.SetFloat("_Radius", val);
            yield return null;
        }
        circleMaterial.SetFloat("_Radius", 0f);
    }

    // 圆形扩大（iris in）：radius 0→1，靠外越快
    private IEnumerator CircleInCoroutine()
    {
        if (circleMaterial == null || circleOverlay == null) yield break;

        float startTime = Time.time;
        while (Time.time - startTime < circleTransitionDuration)
        {
            float t = (Time.time - startTime) / circleTransitionDuration;
            float val = t * t * t; // 三次 ease-in：靠外更快
            circleMaterial.SetFloat("_Radius", val);
            yield return null;
        }
        circleMaterial.SetFloat("_Radius", 1f);

        circleOverlay.gameObject.SetActive(false);
        if (loadingCanvas != null)
            loadingCanvas.SetActive(false);
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        isLoading = true;

        // 入场：圆形缩小
        yield return StartCoroutine(CircleOutCoroutine());

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
            if (progressBar != null)
                progressBar.value = 1f;
        }
        else
        {
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

        // 激活场景（遮罩全黑，玩家看不到）
        async.allowSceneActivation = true;
        yield return null;

        // 隐藏加载UI元素，但保持canvas开启（圆形遮罩需要渲染）
        if (dotsCoroutine != null)
        {
            StopCoroutine(dotsCoroutine);
            dotsCoroutine = null;
        }
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (loadingText != null) loadingText.gameObject.SetActive(false);

        // 退场：圆形扩大揭示新场景（末尾会关闭canvas）
        yield return StartCoroutine(CircleInCoroutine());

        isLoading = false;
    }
}