using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image iconImage;
    public Button button;
    private int slotIndex;
    private DisplayInfo currentData;
    private string currentName;
    private string currentType;
    private GameObject dragIcon;
    private RectTransform dragRect;
    private Canvas rootCanvas;
    private CanvasGroup slotCanvasGroup;

    public bool HasData => currentData != null;
    public string CurrentName => currentName;
    public string CurrentType => currentType;

    public void Setup(string name,string Type, int index, System.Action<int> onClickCallback)
    {

        Debug.Log($"---------name: {name}, type: {Type}---------");
        currentName = name;
        currentType = Type;
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
        currentName = string.Empty;
        currentType = Type;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            return;
        }

        if (slotCanvasGroup == null)
        {
            slotCanvasGroup = GetComponent<CanvasGroup>();
            if (slotCanvasGroup == null)
            {
                slotCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        slotCanvasGroup.blocksRaycasts = false;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragRect = dragIcon.AddComponent<RectTransform>();
        var dragCanvas = dragIcon.AddComponent<Canvas>();
        dragCanvas.overrideSorting = true;
        dragCanvas.sortingLayerID = rootCanvas.sortingLayerID;
        dragCanvas.sortingOrder = 10000;
        var image = dragIcon.AddComponent<Image>();
        image.raycastTarget = false;
        image.sprite = currentData.icon;
        image.preserveAspect = true;
        dragIcon.transform.SetAsLastSibling();
        if (iconImage != null)
        {
            var size = iconImage.rectTransform.rect.size;
            dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
        else
        {
            image.SetNativeSize();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragRect == null || rootCanvas == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint);

        dragRect.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (slotCanvasGroup != null)
        {
            slotCanvasGroup.blocksRaycasts = true;
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        dragRect = null;
        dragIcon = null;
        rootCanvas = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var sourceSlot = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<Slot>() : null;
        if (sourceSlot == null || sourceSlot == this)
        {
            return;
        }

        if (sourceSlot.CurrentType != "Label")
        {
            return;
        }

        var sourceBackpack = sourceSlot.GetComponentInParent<BackpackUI>();
        var sourceLabel = sourceSlot.GetComponentInParent<LabelUI>();
        var targetBackpack = GetComponentInParent<BackpackUI>();
        var targetLabel = GetComponentInParent<LabelUI>();

        if (sourceBackpack != null && targetLabel != null)
        {
            if (!sourceBackpack.IsLabelModeOpen || !targetLabel.IsLabelModeOpen)
            {
                return;
            }

            var label = UIManager.Instance.player?.playerState?.labelBackpack?.Find(lbl => lbl != null && lbl.labelName == sourceSlot.CurrentName);
            if (label == null)
            {
                return;
            }

            if (targetLabel.TryReceiveLabelFromPlayer(label))
            {
                UIManager.Instance.player.playerState.labelBackpack.Remove(label);
                sourceBackpack.ShowBackpack("Label");
                targetLabel.ShowLabel("Label");
            }
            return;
        }

        if (sourceLabel != null && targetBackpack != null)
        {
            if (!sourceLabel.IsLabelModeOpen || !targetBackpack.IsLabelModeOpen)
            {
                return;
            }

            if (sourceLabel.TryTransferLabelToPlayer(sourceSlot.CurrentName, UIManager.Instance.player))
            {
                sourceLabel.ShowLabel("Label");
                targetBackpack.ShowBackpack("Label");
            }
        }
    }

    private bool CanDrag()
    {
        if (!HasData || currentType != "Label")
        {
            return false;
        }

        var sourceBackpack = GetComponentInParent<BackpackUI>();
        if (sourceBackpack != null)
        {
            return sourceBackpack.IsLabelModeOpen && LabelUI.Instance != null && LabelUI.Instance.IsLabelModeOpen;
        }

        var sourceLabel = GetComponentInParent<LabelUI>();
        if (sourceLabel != null)
        {
            return sourceLabel.IsLabelModeOpen && BackpackUI.Instance != null && BackpackUI.Instance.IsLabelModeOpen;
        }

        return false;
    }
}
