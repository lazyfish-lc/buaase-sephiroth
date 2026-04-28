using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class Slot : MonoBehaviour
{
    public Image iconImage;
    public Button button;
    private int slotIndex;
    private DisplayInfo currentData;

    public void Setup(string name,string Type, int index, System.Action<int> onClickCallback)
    {

        Debug.Log($"---------name: {name}, type: {Type}---------");
        currentData = DisplayTable.Instance.GetDisplayInfo(name, Type);
        slotIndex = index;

        if (currentData != null)
        {
            iconImage.sprite = currentData.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        // 清空旧监听，防止重复绑定
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback(slotIndex));
    }
    public void Clean(string name,string Type, int index, System.Action<int> onClickCallback)
    {
        currentData = null;
        slotIndex = index;
        iconImage.sprite = null;
        iconImage.enabled = false;

        // 清空旧监听，防止重复绑定
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback(slotIndex));
    }
    public DisplayInfo GetCurrentData()
    {
        return currentData;
    }
}
