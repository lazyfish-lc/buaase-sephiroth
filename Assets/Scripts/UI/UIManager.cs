using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public MenuUI menuUI;
    public SettingUI settingUI;
    public BackpackUI backpackUI;
    public ActionUI actionUI;
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
    public bool IsAnyUIOpen()
    {
        return menuUI.isOpen || settingUI.isOpen || backpackUI.isOpen ;
    }
}
