using UnityEngine;
using UnityEngine.UI;

public class CustomCursorHotspot : MonoBehaviour
{
    [Header("光标纹理")]
    public Texture2D cursorTexture;          // 拖入你的光标图片

    [Header("热点设置")]
    public bool useAutoHotspot = true;       // 自动计算左上角第一个非透明像素
    public Vector2 manualHotspotOffset = Vector2.zero; // 手动偏移（相对于自动热点，或完全手动）

    [Header("调试")]
    public bool logHotspot = true;

    void Start()
    {
        if (cursorTexture == null)
        {
            Debug.LogError("请指定光标纹理！");
            return;
        }

        // 确保纹理可读（如果未开启Read/Write会在运行时尝试修复，但最好提前在导入设置中勾选）
        MakeTextureReadable(cursorTexture);

        Vector2 hotspot = Vector2.zero;

        if (useAutoHotspot)
        {
            hotspot = FindTopLeftNonTransparentPixel(cursorTexture);
            if (logHotspot)
                Debug.Log($"自动计算的热点: ({hotspot.x}, {hotspot.y})");

            // 应用手动偏移（如果需要）
            hotspot += manualHotspotOffset;
        }
        else
        {
            hotspot = manualHotspotOffset;
        }

        // 确保热点在纹理范围内
        hotspot.x = Mathf.Clamp(hotspot.x, 0, cursorTexture.width - 1);
        hotspot.y = Mathf.Clamp(hotspot.y, 0, cursorTexture.height - 1);

        // 设置鼠标光标
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);

        if (logHotspot)
            Debug.Log($"最终应用的热点: ({hotspot.x}, {hotspot.y})，纹理尺寸: {cursorTexture.width}x{cursorTexture.height}");
    }

    /// <summary>
    /// 扫描纹理，返回左上角第一个 alpha > 0.1 的像素坐标
    /// </summary>
    private Vector2 FindTopLeftNonTransparentPixel(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        int w = tex.width;
        int h = tex.height;

        for (int y = 0; y < h; y++)       // 从上到下
        {
            for (int x = 0; x < w; x++)   // 从左到右
            {
                if (pixels[y * w + x].a > 0.1f)
                {
                    return new Vector2(x, y);
                }
            }
        }

        // 全透明则默认返回中心
        Debug.LogWarning("未找到任何非透明像素，使用纹理中心作为热点。");
        return new Vector2(w / 2, h / 2);
    }

    /// <summary>
    /// 确保纹理可读（如果导入时未开启Read/Write，尝试复制一份可读纹理）
    /// </summary>
    private void MakeTextureReadable(Texture2D original)
    {
        try
        {
            // 尝试读取像素检查是否可读
            original.GetPixels();
        }
        catch (UnityException)
        {
            Debug.LogWarning("光标纹理未开启 Read/Write，正在尝试生成可读副本...");
            // 创建可读副本
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height);
            Graphics.Blit(original, rt);
            Texture2D readableTex = new Texture2D(original.width, original.height);
            readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTex.Apply();
            RenderTexture.ReleaseTemporary(rt);
            cursorTexture = readableTex;
            Debug.Log("已生成可读纹理副本，请确保原图导入设置中勾选 Read/Write 以避免性能损失。");
        }
    }
}