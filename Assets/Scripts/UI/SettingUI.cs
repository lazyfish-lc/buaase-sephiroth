using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public CanvasGroup SettingCanvas;

    public Sprite MusicOnSprite;
    public Sprite MusicOffSprite;
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;

    public Button MusicButton;
    public Button SoundButton;

    public Slider MusicSlider;
    public Slider SoundSlider;

    public TMP_Text MusicVolumeNumber;
    public TMP_Text SoundVolumeNumber;
    private float LastMusicVolume;
    private float LastSoundVolume;

    private bool isMusicOn;
    private bool isSoundOn;

    public static SettingUI Instance;
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
    void Start()
    {
        SettingCanvas.alpha = 0;
        SettingCanvas.interactable = false;
        SettingCanvas.blocksRaycasts = false;
    }
    void Update()
    {
        MusicVolumeNumber.text = GetMusicVolume(MusicSlider.value).ToString();
        SoundVolumeNumber.text = GetSoundVolume(SoundSlider.value).ToString();
    }

    void LoadPreferences()
    {
        // 音乐和音效
        MusicSlider.value = LastMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        SoundSlider.value = LastSoundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.8f);
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        MusicButton.image.sprite = isMusicOn ? MusicOnSprite : MusicOffSprite;
        SoundButton.image.sprite = isSoundOn ? SoundOnSprite : SoundOffSprite;
        MusicSlider.interactable = isMusicOn;
        SoundSlider.interactable = isSoundOn;
    }

    public void OpenAndClose()
    {
        
        if(SettingCanvas.alpha == 0)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
    public void Open()
    {
        SettingCanvas.alpha = 1;
        SettingCanvas.interactable = true;
        SettingCanvas.blocksRaycasts = true;
        LoadPreferences();
    }
    public void Close()
    {
        SettingCanvas.alpha = 0;
        SettingCanvas.interactable = false;
        SettingCanvas.blocksRaycasts = false;
    }

    public void OpenAndCloseMusic()
    {
        if(isMusicOn)
        {
            MusicButton.image.sprite = MusicOffSprite;
            isMusicOn = false;
            LastMusicVolume = MusicSlider.value;
            MusicSlider.value = 0;
            MusicSlider.interactable = false;
        }
        else
        {
            MusicButton.image.sprite = MusicOnSprite;
            isMusicOn = true;
            MusicSlider.value = LastMusicVolume;
            MusicSlider.interactable = true;
        }

    }

    public void OpenAndCloseSound()
    {
        if(isSoundOn)
        {
            SoundButton.image.sprite = SoundOffSprite;
            isSoundOn = false;
            LastSoundVolume = SoundSlider.value;
            SoundSlider.value = 0;
            SoundSlider.interactable = false;
        }
        else
        {
            SoundButton.image.sprite = SoundOnSprite;
            isSoundOn = true;
            SoundSlider.value = LastSoundVolume;
            SoundSlider.interactable = true;
        }
        
    }

    public void Save()
    {
        // 保存设置逻辑（例如保存到PlayerPrefs）
        Debug.Log("设置已保存");
        //保存音量设置
        Debug.Log("音乐音量: " + GetMusicVolume(MusicSlider.value));
        Debug.Log("音效音量: " + GetSoundVolume(SoundSlider.value));
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("IsSoundOn", isSoundOn ? 1 : 0);
        // 触发应用设置变化的事件（例如调整音量）
        // AudioManager.Instance.SetMusicVolume(MusicSlider.value);
    }
    public int GetMusicVolume(float volume)
    {
        return (int)(volume * 100);
    }
    public int GetSoundVolume(float volume)
    {
        return (int)(volume * 100);
    }

    public void KeySetting()
    {
        Debug.Log("打开按键设置界面");
        //KeySettingUI.Instance.OpenAndClose();
    }
    
}
