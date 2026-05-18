using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PlayerSmallObject player;
    public UISFXPlayer SFXPlayer;
    public MenuUI menuUI;
    public SettingUI settingUI;
    public BackpackUI backpackUI;
    public ActionUI actionUI;
    public StatusUI statusUI;
    public PropertyUI propertyUI;
    public LabelUI labelUI;
    public KeysetUI keysetUI;
    public HintUI hintUI;
    public GuideUI guideUI;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void OnDisable()
    {
        if (player != null)
        {
            player.ItemAddedToBackpack -= HandlePlayerItemAdded;
            player.ItemRemovedFromBackpack -= HandlePlayerItemRemoved;
        }

        DialogueNodeActionSOActions.TutorialPopupRequested -= HandleDialogueTutorialPopup;
    }

    public void HandlePlayerItemAdded(Item item)
    {
        if (item == null) return;
        Debug.Log($"UI收到物品新增事件: {item.itemName}");
    }

    public void HandlePlayerItemRemoved(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0) return;
        Debug.Log($"UI收到物品移除事件: {itemName} x{amount}");
    }

    public void HandleDialogueTutorialPopup(NPCObject npc, string actionId, TutorialContent content)
    {
        if (npc == null || string.IsNullOrWhiteSpace(actionId)) return;
        string title = content != null ? content.title : string.Empty;
        string body = content != null ? content.body : string.Empty;
        Debug.Log($"UI收到对话教学弹窗事件: npc={npc.name}, actionId={actionId}, title={title}, body={body}");
    }

    public void PlayOpenSFX()
    {
        if (SFXPlayer != null)
        {
            SFXPlayer.PlayOpenSFX();
        }
    }
    public void PlayCloseSFX()
    {
        if (SFXPlayer != null)
        {
            SFXPlayer.PlayCloseSFX();
        }
    }
    public void PlayClickSFX()
    {
        if (SFXPlayer != null)
        {
            SFXPlayer.PlayClickSFX();
        }
    }
}
