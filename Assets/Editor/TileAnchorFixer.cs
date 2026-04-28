using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class VisualCenterFixer : Editor
{
    [MenuItem("Tools/战斗系统/修复瓦片视觉居中 (Match Anchor to Pivot)")]
    public static void FixVisualCentering()
    {
        // 1. 获取选中的 Tile 资源
        Object[] selectedTiles = Selection.GetFiltered(typeof(Tile), SelectionMode.DeepAssets);
        
        if (selectedTiles.Length == 0)
        {
            Debug.LogWarning("请先在 Project 窗口选中需要修复的 Tile 资源或文件夹！");
            return;
        }

        int count = 0;
        foreach (Object obj in selectedTiles)
        {
            Tile tile = obj as Tile;
            if (tile == null || tile.sprite == null) continue;

            // 2. 获取 Sprite 的归一化 Pivot 坐标
            // 例如：底部中心会返回 (0.5, 0)
            Vector2 spritePivot = GetNormalizedPivot(tile.sprite);

            // 3. 使用 SerializedObject 修改私有属性 m_Anchor，确保不破坏 GUID
            SerializedObject so = new SerializedObject(tile);
            SerializedProperty anchorProp = so.FindProperty("m_Anchor");

            if (anchorProp != null)
            {
                // 将 Tile 的 Anchor 设为与 Sprite Pivot 相同
                // 这样图片的中心就会自动对齐到格子的中心
                anchorProp.vector3Value = new Vector3(spritePivot.x, spritePivot.y, 0);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(tile);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>【修复成功】已将 {count} 个瓦片的视觉中心对齐至格子中心。</color>");
    }

    private static Vector2 GetNormalizedPivot(Sprite sprite)
    {
        // 计算归一化 Pivot (0-1 范围)
        Vector2 pivotPixels = sprite.pivot;
        Rect rect = sprite.rect;
        return new Vector2(pivotPixels.x / rect.width, pivotPixels.y / rect.height);
    }
}