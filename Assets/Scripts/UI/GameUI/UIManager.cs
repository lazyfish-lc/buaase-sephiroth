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
